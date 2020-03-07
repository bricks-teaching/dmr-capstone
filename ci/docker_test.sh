#!/usr/bin/env bash

set -e

docker run \
  -e UNITY_LICENSE_CONTENT \
  -e TEST_PLATFORM \
  -e UNITY_USERNAME \
  -e UNITY_PASSWORD \
  -e TRAVIS_PULL_REQUEST \
  -e TRAVIS_REPO_SLUG \
  -e GITHUB_TOKEN \
  -w /project/ \
  -v $(pwd):/project/ \
  $IMAGE_NAME \
  /bin/bash -c "apt-get update && apt-get -qq -y install curl && /project/ci/before_script.sh && /project/ci/test.sh"
