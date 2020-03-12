#!/usr/bin/env bash

echo ""
echo -en "travis_fold:start:prepare_workspace\r"
echo "Preparing workspace..."
# 66087b1c9a02
openssl aes-256-cbc -K $encrypted_66087b1c9a02_key -iv $encrypted_66087b1c9a02_iv -in ./Unity_v2019.x.ulf.enc -out ./Unity_v2019.x.ulf -d
export UNITY_LICENSE_CONTENT=`cat Unity_v2019.x.ulf`
rm Unity_v2019.x.ulf
docker pull $IMAGE_NAME
echo -e "machine github.com\n  login $GITHUB_TOKEN" > ~/.netrc
echo -en "\ntravis_fold:end:prepare_workspace\r"