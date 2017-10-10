const parse = require("./customFormatter").parse;

describe("Custom i18n formatter", () => {

  it('able to parse tokens with text ""Enter the {{building}} with me {{inside}} force""', () => {
    const result = parse("Enter the {{building}} with me {{inside}} force");

    expect(result).toEqual(
      [{ "type": "text", "value": "Enter the " },
      { "type": "named", "value": "building" },
      { "type": "text", "value": " with me " },
      { "type": "named", "value": "inside" },
      { "type": "text", "value": " force" }])
  });

  it('able to parse single token {{building}}', () => {
    const result = parse("{{building}}");

    expect(result).toEqual([
      { "type": "named", "value": "building" }
    ])
  });

  it('able to parse several tokens "{{token}}{{building}}"', () => {
    const result = parse("{{token}}{{building}}");

    expect(result).toEqual(
      [
        { "type": "named", "value": "token" },
        { "type": "named", "value": "building" }
      ])
  });
});