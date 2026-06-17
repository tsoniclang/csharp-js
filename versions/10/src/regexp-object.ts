import type { int } from "@tsonic/core/types.js";
import {
  Group,
  Match,
  Regex,
  RegexOptions,
} from "@tsonic/dotnet/System.Text.RegularExpressions.js";

const toRegexOptions = (flags: string): RegexOptions => {
  let options = RegexOptions.None;

  for (let i = 0; i < flags.length; i += 1) {
    const flag = flags[i];

    if (flag === "i") {
      options = (options | RegexOptions.IgnoreCase) as RegexOptions;
      continue;
    }

    if (flag === "m") {
      options = (options | RegexOptions.Multiline) as RegexOptions;
      continue;
    }

    if (flag === "s") {
      options = (options | RegexOptions.Singleline) as RegexOptions;
      continue;
    }
  }

  return options;
};

const toMatchArray = (
  match: Match
): string[] => {
  const enumerator = match.Groups.GetEnumerator();
  const values: string[] = [];

  while (enumerator.MoveNext()) {
    values.push((enumerator.Current as Group).Value);
  }

  return values;
};

export class RegExp {
  source: string;
  flags: string;

  regex: Regex;

  constructor(pattern: string | RegExp, flags: string = "") {
    if (pattern instanceof RegExp) {
      this.source = pattern.source;
      this.flags = flags === "" ? pattern.flags : flags;
    } else {
      this.source = pattern;
      this.flags = flags;
    }

    this.regex = new Regex(this.source, toRegexOptions(this.flags));
  }

  exec(input: string): string[] | null {
    const match = this.regex.Match(input);
    if (!match.Success) {
      return null;
    }

    return toMatchArray(match);
  }

  test(input: string): boolean {
    return this.regex.IsMatch(input);
  }

  toString(): string {
    return `/${this.source}/${this.flags}`;
  }
}
