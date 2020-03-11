#!/usr/bin/env bash

echo -e "machine github.com\n  login $GITHUB_TOKEN" > ~/.netrc

echo Listing all tags.
git tag
export COMMIT_HASH="$(git log --format=%h -1)"
echo Latest tagged version: `git tag -l v* | tail -n 1`
export MAJOR_VER=0  # this is 0 until maybe milestone 3
export MINOR_VER=`git tag -l v* | tail -n 1 | perl -p0e 's/v\d+\.(\d+)\.(\d+)/\1/'`
echo Minor version number: ${MINOR_VER}
export PATCH_VER=`git tag -l v* | tail -n 1 | perl -p0e 's/v\d+\.(\d+)\.(\d+)/\2/'`
echo Patch version number: ${PATCH_VER}
export PSEM_VER=`echo v${MAJOR_VER}.${MINOR_VER}.$(( PATCH_VER + 1 ))`
echo Next version tag is ${PSEM_VER}
git tag -a ${PSEM_VER} -m "These releases were built automatically for OpenGL from commit ${COMMIT_HASH}."
git push origin refs/tags/${PSEM_VER}
