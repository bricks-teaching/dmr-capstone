#!/usr/bin/env bash

CURRENT_HEAD=`git log -1 @ --pretty="%H"`
PACKAGE_HEAD=`cat rls/$1_head`

if [[ "${CURRENT_HEAD}" == "${PACKAGE_HEAD}" ]]; then
    echo Packaged release is for this commit, continuing with deploy.
else
    echo Packaged release is NOT for this commit!
    echo ========================================
    echo Current head:
    git log -1 @
    echo ========================================
    echo Package head:
    git log -1 ${PACKAGE_HEAD}
    echo ========================================
    echo Aborting deployment.
    exit 1
fi
