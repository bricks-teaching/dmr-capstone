#!/usr/bin/env bash

TEST_PLATFORM=editmode bash ./ci/docker_test.sh
TEST_PLATFORM=playmode bash ./ci/docker_test.sh