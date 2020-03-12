#!/usr/bin/env bash

export GIT_TRACE=1
export GIT_TRANSFER_TRACE=1

echo ""
echo -en "travis_fold:start:lfs\r"
echo Pulling LFS...
git lfs pull
echo -en "travis_fold:end:lfs\r"
echo ""
