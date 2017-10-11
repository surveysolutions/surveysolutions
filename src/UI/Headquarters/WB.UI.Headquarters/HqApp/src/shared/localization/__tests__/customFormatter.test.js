const tokenize = require("../customFormatter").tokenize;

describe("Custom i18n formatter", () => {

  it('able to parse tokens with text ""Enter the {{building}} with me {{inside}} force""', () => {
    const result = tokenize("Enter the {{building}} with me {{inside}} force");

    expect(result).toEqual(
      [{ "type": "text", "value": "Enter the " },
      { "type": "named", "value": "building" },
      { "type": "text", "value": " with me " },
      { "type": "named", "value": "inside" },
      { "type": "text", "value": " force" }])
  });

  it('able to parse tokens with spaces inside tokens ""Enter the {{  building }} with me {{  inside   }} force""', () => {
    const result = tokenize("Enter the {{building}} with me {{inside}} force");

    expect(result).toEqual(
      [{ "type": "text", "value": "Enter the " },
      { "type": "named", "value": "building" },
      { "type": "text", "value": " with me " },
      { "type": "named", "value": "inside" },
      { "type": "text", "value": " force" }])
  });

  it('able to parse single token {{building}}', () => {
    const result = tokenize("{{building}}");

    expect(result).toEqual([
      { "type": "named", "value": "building" }
    ])
  });

  it('able to parse several tokens "{{token}}{{building}}"', () => {
    const result = tokenize("{{token}}{{building}}");

    expect(result).toEqual(
      [
        { "type": "named", "value": "token" },
        { "type": "named", "value": "building" }
      ])
  });

  it('able to parse list tokens "{0}{1 }{ 2}{ 3 } inner text {{building}}"', () => {
    const result = tokenize("{0}{1 }{ 2}{ 3 } inner text {{building}}");

    expect(result)
      .toEqual(
      [
        { "type": "list", "value": "0" },
        { "type": "list", "value": "1" },
        { "type": "list", "value": "2" },
        { "type": "list", "value": "3" },
        { "type": "text", "value": " inner text " },
        { "type": "named", "value": "building" }
      ])
  });
});
