#!/usr/bin/env bash

set -x

echo ""
echo -en "travis_fold:start:run_tests\r"
echo "Testing for $TEST_PLATFORM"

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd)/fiscal-shock \
  -runTests \
  -testPlatform $TEST_PLATFORM \
  -testResults $(pwd)/$TEST_PLATFORM-results.xml \
  -logFile /dev/stdout \
  -batchmode

UNITY_EXIT_CODE=$?

echo -en "\ntravis_fold:end:run_tests\r"

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
  bash $(pwd)/ci/run_on_failed_tests.sh "${TEST_PLATFORM}" "`cat ${TEST_PLATFORM}-results.xml`"
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
  bash $(pwd)/ci/run_on_failed_tests.sh "${TEST_PLATFORM}" "`cat ${TEST_PLATFORM}-results.xml`"
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

cat $(pwd)/$TEST_PLATFORM-results.xml | grep test-run | grep Passed
exit $UNITY_TEST_EXIT_CODE
