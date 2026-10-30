import pluginVue from 'eslint-plugin-vue'
import prettierConfig from 'eslint-config-prettier'

const ignores = [
    'dist/**',
    'js/**',
    '**/*.test.js',
    'vendor/**',
    'vue.config.js',
    'tools/**',
]

const globals = {
    $: 'readonly',
    jQuery: 'readonly',
    google: 'readonly',
}

const baseRules = {
    'comma-dangle': ['error', {
        arrays: 'always-multiline',
        objects: 'always-multiline',
        imports: 'never',
        exports: 'never',
        functions: 'never',
    }],
    indent: ['error', 4, { SwitchCase: 1 }],
    quotes: ['error', 'single'],
    semi: ['error', 'never'],
    'no-unused-vars': 'off',
    'no-trailing-spaces': 'error',
}

const vueRules = {
    'vue/no-unused-vars': 'off',
    'vue/require-v-for-key': 'off',
    'vue/html-indent': ['error', 4, {
        attribute: 1,
        alignAttributesVertically: false,
        closeBracket: 0,
    }],
    'vue/html-closing-bracket-newline': ['error', {
        singleline: 'never',
        multiline: 'never',
    }],
    'vue/singleline-html-element-content-newline': ['error', {
        ignoreWhenNoAttributes: true,
        ignoreWhenEmpty: true,
        ignores: ['pre', 'textarea', 'p', 'span', 'li'],
    }],
    'vue/multi-word-component-names': 'off',
    'vue/no-reserved-component-names': 'warn',
    'vue/max-attributes-per-line': ['error', {
        singleline: 1,
        multiline: 1,
    }],
}

const sharedLanguageOptions = {
    ecmaVersion: 'latest',
    sourceType: 'module',
    globals,
}

export default [
    {
        ignores,
    },
    {
        files: ['**/*.js'],
        languageOptions: sharedLanguageOptions,
        rules: {
            ...prettierConfig.rules,
            ...baseRules,
        },
    },
    ...pluginVue.configs['flat/essential'].map(config => ({
        ...config,
        files: ['**/*.vue'],
        languageOptions: {
            ...config.languageOptions,
            ...sharedLanguageOptions,
            globals: {
                ...(config.languageOptions?.globals ?? {}),
                ...globals,
            },
            parserOptions: {
                ...(config.languageOptions?.parserOptions ?? {}),
                ecmaVersion: 'latest',
                sourceType: 'module',
            },
        },
        rules: {
            ...(config.rules ?? {}),
            ...prettierConfig.rules,
            ...baseRules,
            ...vueRules,
        },
    })),
]