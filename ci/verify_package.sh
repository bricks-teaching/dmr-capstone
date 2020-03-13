#!/usr/bin/env bash

CURRENT_HEAD=`git log -1 @ --pretty="%H"`
export PACKAGE_HEAD=`cat rls_$1/$1_head`

if [[ "${CURRENT_HEAD}" == "${PACKAGE_HEAD}" ]]; then
    echo Packaged release is for this commit, continuing with deploy.
    export PACKAGE_HEAD=''
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
    export BAD_PACKAGE=1
    export PACKAGE_HEAD=''
    exit 1
fi
