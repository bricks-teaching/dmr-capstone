#!/usr/bin/env bash

if test "$TRAVIS_PULL_REQUEST" != "false"; then
    platform=$1
    xml=`echo $2 | sed -e "s:\":':gm"`

    echo ${xml}

    curl -H "Authorization: token ${GITHUB_TOKEN}" -X POST \
    -d "{\"body\": \"${platform} tests failed:\n\`\`\`xml\n ${xml} \n\`\`\`\"}" \
    "https://api.github.com/repos/${TRAVIS_REPO_SLUG}/issues/${TRAVIS_PULL_REQUEST}/comments"
fi
