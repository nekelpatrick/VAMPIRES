module.exports = {
  root: true,
  env: {
    browser: true,
    es2023: true,
    node: true
  },
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 'latest',
    sourceType: 'module',
    tsconfigRootDir: __dirname
  },
  plugins: ['@typescript-eslint', 'import'],
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:import/recommended',
    'plugin:import/typescript'
  ],
  rules: {
    'import/order': [
      'error',
      {
        groups: [['builtin', 'external'], ['internal'], ['parent', 'sibling', 'index']],
        'newlines-between': 'always',
        alphabetize: { order: 'asc', caseInsensitive: true }
      }
    ],
    'import/newline-after-import': ['error', { count: 1 }],
    '@typescript-eslint/consistent-type-imports': [
      'error',
      { prefer: 'type-imports', disallowTypeAnnotations: false }
    ]
  }
}

