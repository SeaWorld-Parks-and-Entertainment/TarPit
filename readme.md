# A proof-of-work-based HTTP API ratelimiter

## Backstory

This project comes out of 2 common challenges we experience in our line of work: DDOS and brute-forcing our APIs. This rate-limiting system solves these problems by imposing escalating workloads on bad actors while allowing good-faith users of our websites and APIs to do so at sensible rates without imposing any work on them.

## Design Principles

### Asymmetric defense

This rate limiter must impose more work on attackers than it does on defenders, and the defenders must be able to control the amount of work demanded of attackers dynamically to respond in real-time to attacks.

### Shortcomings of existing systems

In sliding-window rollup ratelimiting systems, attackers are able to impose significant load by opening massive numbers of connections simultaneously. Defenders must record new calls and calculate request count within the window, which can be a significant amount of work, while attackers only cost is that of TCP connections and IP addresses.

This system sets out to reduce to a very low floor the amount of work attackers can force defenders to perform without themselves having to perform possibly significantly more work, thus effectively solving the DDOS problem.

### System Design

* All responses have a new `difficulty` header, that indicates how many leading zeros a caller-provided nonce must satisfy.
* All requests must come with a nonce in a request header.
* Provided nonces must hash to a value with a count of leading zeros equal to `difficulty`.
* As callers exceed the specified rate limit, difficulty per caller is incremented, until callers must do so much work per request that their request rate must drop below the ratelimit threshold.
* Nonces may not be reused.

# Credits and Acknowledgements

SeaWorld Parks and Entertainment for graciously open-sourcing this project. I needed to boot up on .NET architectures and the CLR in general, and this project was a perfect fit. Rather than letting it languish in some drawer, we've decided to open source this implementation so that others can learn from it.

Richard June and Brandon Schultz for .NET wisdom.

Benjamin van der Veen and Michael Trinque for being sounding boards and always attempting to poke holes in design and implementation.
