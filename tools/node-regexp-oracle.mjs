import { readFileSync } from "node:fs";

const request = JSON.parse(readFileSync(0, "utf8"));

function serializeMatch(match) {
  if (match === null) {
    return null;
  }
  return {
    value: match[0],
    index: match.index,
    input: match.input,
    length: match.length,
    groups: Array.from(match, (value) => value ?? null),
  };
}

try {
  const regexp = new RegExp(request.pattern, request.flags ?? "");
  const observations = [];
  for (const operation of request.operations ?? []) {
    if (Object.hasOwn(operation, "lastIndex") && operation.lastIndex !== null && operation.lastIndex !== undefined) {
      regexp.lastIndex = operation.lastIndex;
    }
    if (operation.kind === "test") {
      observations.push({
        kind: "test",
        result: regexp.test(operation.input),
        lastIndex: regexp.lastIndex,
      });
      continue;
    }
    if (operation.kind === "exec") {
      observations.push({
        kind: "exec",
        result: serializeMatch(regexp.exec(operation.input)),
        lastIndex: regexp.lastIndex,
      });
      continue;
    }
    throw new Error(`unknown operation kind ${operation.kind}`);
  }
  process.stdout.write(JSON.stringify({
    kind: "ok",
    source: regexp.source,
    flags: regexp.flags,
    global: regexp.global,
    hasIndices: regexp.hasIndices,
    ignoreCase: regexp.ignoreCase,
    multiline: regexp.multiline,
    dotAll: regexp.dotAll,
    unicode: regexp.unicode,
    unicodeSets: regexp.unicodeSets,
    sticky: regexp.sticky,
    observations,
  }));
} catch (error) {
  process.stdout.write(JSON.stringify({
    kind: "throw",
    name: error?.name ?? Object.prototype.toString.call(error),
    message: String(error?.message ?? error),
  }));
}
