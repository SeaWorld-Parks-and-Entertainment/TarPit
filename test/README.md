# High level testing tools

* generating nonces
* load testing

## Generating Nonces

Included in this directory is a script for generating nonces. It is a shell script that runs on a Mac of reasonably recent vintage, where "reasonably recent vintage" is an opaque definition left to my discretion.

One runs it from a typical bash prompt, `./generate_diff_satisfying_nonces.sh`, and it outputs valid nonces to `stdout`.

The script has 2 user-facing variables:
* `DESIRED_NONCE_COUNT`
  Controls how many nonces the script generates before exiting.
* `DESIRED_DIFFICULTY`
  Controls how many leading zeros a nonce's hash must have to be considered valid.
