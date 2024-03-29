module.exports = {
    root: true,
    env: {
        es2021: true
    },
    extends: [
        'plugin:vue/recommended',
        'eslint:recommended',
        'prettier/vue',
        'plugin:prettier/recommended'
    ],
    "rules": {
        "comma-dangle": [
            "error",
            {
                "arrays": "always-multiline",
                "objects": "always-multiline",
                "imports": "never",
                "exports": "never",
                "functions": "never"
            }
        ],
        "indent": [
            "error",
            4,
            {
                "SwitchCase": 1
            }
        ],
        "quotes": [
            "error",
            "single"
        ],
        "semi": [
            "error",
            "never"
        ],
        "no-unused-vars": "off",
        "vue/no-unused-vars": "off",
        "vue/require-v-for-key": "off",
        "vue/html-indent": [
            "error",
            4,
            {
                "attribute": 1,
                "alignAttributesVertically": false,
                "closeBracket": 0,
            }
        ],
        "vue/html-closing-bracket-newline": [
            "error",
            {
                "singleline": "never",
                "multiline": "never"
            }
        ],
        "vue/singleline-html-element-content-newline": [
            "error",
            {
                "ignoreWhenNoAttributes": true,
                "ignoreWhenEmpty": true,
                "ignores": [
                    "pre",
                    "textarea",
                    "p",
                    "span",
                    "li"
                ]
            }
        ],
        "vue/multi-word-component-names": "off",
        "vue/no-reserved-component-names": "warn",
        "vue/max-attributes-per-line": [
            "error",
            {
                "singleline": 1,
                "multiline": 1
            }
        ],
        "no-trailing-spaces": "error",
    },
    "parserOptions": {
        "parser": "babel-eslint"
    },
    "globals": {
        "$": true,
        "jQuery": true,
        "google": "readonly"
    },
    "ignorePatterns": [
        "dist/**",
        "js/**",
        "**/*.test.js",
        "vendor/**",
        "vue.config.js",
        "tools/**"
    ]
};
