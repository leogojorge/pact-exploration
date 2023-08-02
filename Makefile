SHELL = sh -xv
GITHUB_ORG="pactflow"
PACTICIPANT="pact-exploration"
GITHUB_WEBHOOK_UUID := "04510dc1-7f0a-4ed2-997d-114bfa86f8ad"
PACT_CLI="docker run --rm -v ${PWD}:${PWD} -e PACT_BROKER_BASE_URL -e PACT_BROKER_TOKEN pactfoundation/pact-cli:latest"
#PACT_CLI="docker run --rm -v C:\Projetos\pact-exploration:/app pactfoundation/pact-cli"
#PACT_CLI="docker run --rm -w ${PWD} -v ${PWD}:${PWD} pactfoundation/pact-cli:latest"

.EXPORT_ALL_VARIABLES:
GIT_COMMIT?=$(shell git rev-parse HEAD)
GIT_BRANCH?=$(shell git rev-parse --abbrev-ref HEAD)
ENVIRONMENT?=production

# Only deploy from master (to production env) or test (to test env)
ifeq ($(GIT_BRANCH),master)
	ENVIRONMENT=production
	DEPLOY_TARGET=deploy
else
	ifeq ($(GIT_BRANCH),test)
		ENVIRONMENT=test
		DEPLOY_TARGET=deploy
	else
		DEPLOY_TARGET=no_deploy
	endif
endif

build-and-run:
	make restore
	make build
	make run_tests
	make publish_pacts
	make can_i_deploy

build:
	dotnet build

restore:
	dotnet restore

run_tests: .env
	@echo "\n========== STAGE: test (pact) ==========\n"
	dotnet test

publish_pacts: .env
	@echo "========== STAGE: publish pacts =========="
	@"${PACT_CLI}" publish ${PWD}/pacts \
	  --consumer-app-version ${GIT_COMMIT} \
	  --branch ${GIT_BRANCH} \
	  --broker-base-url ${PACT_BROKER_BASE_URL}

can_i_deploy: .env
	@echo "\n========== STAGE: can-i-deploy? ==========\n"
	@"${PACT_CLI}" broker can-i-deploy \
	  --pacticipant ${PACTICIPANT} \
	  --version ${GIT_COMMIT} \
	  --to-environment ${ENVIRONMENT} \
	  --retry-while-unknown 30 \
	  --retry-interval 10

deploy_app:
	@echo "\n========== STAGE: deploy ==========\n"
	@echo "Deploying to ${ENVIRONMENT}"

## =====================
## Deploy tasks
## =====================

record_deployment: .env
	@"${PACT_CLI}" broker record-deployment --pacticipant ${PACTICIPANT} --version ${GIT_COMMIT} --environment ${ENVIRONMENT}

## =====================
## PactFlow set up tasks
## =====================

# This should be called once before creating the webhook
# with the environment variable GITHUB_TOKEN set
create_github_token_secret:
	@curl -v -X POST ${PACT_BROKER_BASE_URL}/secrets \
	-H "Authorization: Bearer ${PACT_BROKER_TOKEN}" \
	-H "Content-Type: application/json" \
	-H "Accept: application/hal+json" \
	-d  "{\"name\":\"githubCommitStatusToken\",\"description\":\"Github token for updating commit statuses\",\"value\":\"${GITHUB_TOKEN}\"}"

# This webhook will update the Github commit status for this commit
# so that any PRs will get a status that shows what the status of
# the pact is.
create_or_update_github_commit_status_webhook:
	@"${PACT_CLI}" \
	  broker create-or-update-webhook \
	  'https://api.github.com/repos/pactflow/example-consumer/statuses/$${pactbroker.consumerVersionNumber}' \
	  --header 'Content-Type: application/json' 'Accept: application/vnd.github.v3+json' 'Authorization: token $${user.githubCommitStatusToken}' \
	  --request POST \
	  --data @${PWD}/pactflow/github-commit-status-webhook.json \
	  --uuid ${GITHUB_WEBHOOK_UUID} \
	  --consumer ${PACTICIPANT} \
	  --contract-published \
	  --provider-verification-published \
	  --description "Github commit status webhook for ${PACTICIPANT}"

test_github_webhook:
	@curl -v -X POST ${PACT_BROKER_BASE_URL}/webhooks/${GITHUB_WEBHOOK_UUID}/execute -H "Authorization: Bearer ${PACT_BROKER_TOKEN}"

no_deploy:
	@echo "Not deploying as not on master branch"

## ======================
## Misc
## ======================

.env:
	touch .env

output:
	mkdir -p ./pacts
	touch ./pacts/tmp

clean: output
	rm pacts/*