#!/usr/bin/env bash


echo ""
echo -en "travis_fold:start:docs\r"
echo Generating documentation...

pushd .
cd sphinx
pip install -r requirements.txt
git lfs pull -I ../fiscal-shock/Assets/UserInterface/dmr-icon128.png
make html
popd
touch docs/html/.nojekyll

echo -en "\ntravis_fold:end:docs\r"