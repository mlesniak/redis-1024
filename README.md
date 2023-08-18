# Redis in 1024 lines of code

## Goal

Implement as much functionality of a redis server as possible in 1024 lines of 
C# code (according to `cloc`). Test code (in a separate module) does not count.

## Approach

- Listen to connection
- Implement commands until we run out of lines

Received

```
*2          an array with two elements
$7          string of 7 bytes follows
COMMAND
$4          string 4 bytes follows
DOCS
```

Client sent `COMMAND DOCS` and expects something in return?

## References

 - https://redis.io/docs/reference/protocol-spec/
