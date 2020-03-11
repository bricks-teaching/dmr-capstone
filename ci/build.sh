#!/usr/bin/env bash

set -e
set -x

echo ""
echo -en "travis_fold:start:build_unity\r"
echo "Building for $BUILD_TARGET"

export BUILD_PATH=/project/Builds/$BUILD_DIR/
mkdir -p $BUILD_PATH

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd)/fiscal-shock \
  -batchmode \
  -quit \
  ${OPTS}\
  -buildTarget $BUILD_TARGET \
  -customBuildTarget $BUILD_TARGET \
  -customBuildName $BUILD_NAME \
  -customBuildPath $BUILD_PATH \
  -executeMethod BuildCommand.PerformBuild \
  -logFile /dev/stdout

UNITY_EXIT_CODE=$?
echo -en "\ntravis_fold:end:build_unity\r"

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Build succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -neq 0 ]; then
  echo "Build failed with exit code $UNITY_EXIT_CODE";
fi

ls -la $BUILD_PATH

[ -n "$(ls -A $BUILD_PATH)" ] # fail job if build folder is empty
