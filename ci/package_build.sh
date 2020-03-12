#!/usr/bin/env bash

mkdir -p rls
echo `git log -1 @ --pretty="%H"` > rls/$1_head
pushd .
cd Builds

case $1 in
    win64)
        zip -r -9 ../rls/FiscalShock-$1.zip FiscalShock-$1;;
    *)
        tar -czf ../rls/FiscalShock-$1.tar.gz FiscalShock-$1;;
esac

popd
