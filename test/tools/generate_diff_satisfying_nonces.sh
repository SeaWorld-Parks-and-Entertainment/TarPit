#!/usr/bin/env bash -eu

DESIRED_NONCE_COUNT=100
DESIRED_DIFFICULTY=1

GENERATED_NONCE_COUNT=0

while [ $GENERATED_NONCE_COUNT -lt $DESIRED_NONCE_COUNT ]
do
    nonce=$(xxd -l16 -p /dev/urandom)
    hash=$(printf $(printf "$nonce" | shasum -a 256))
    diff_string=$(printf '%*s' "$DESIRED_DIFFICULTY" | tr ' ' 0)
    if [[ "$hash" = "$diff_string"* ]]
    then
        printf '%s %s\n' "$nonce" "$hash"
        ((GENERATED_NONCE_COUNT=$GENERATED_NONCE_COUNT+1))
    fi
done
