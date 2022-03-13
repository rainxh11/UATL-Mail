module.exports = {
  root: true,
  env:{
	es2021: true
  },
  extends: [
    '@indielayer/eslint-config-vue'
  ],
  rules: {
    'vue/html-self-closing': 'off',
    'max-len': 'off',
    semi: ['error', 'never'],
    'arrow-parens': 'off',
    'linebreak-style': 'off',
    'global-require': 'off',
    'newline-before-return': 'error',
    'import/newline-after-import': 'off',
    'no-unused-vars': 'off',
    'padding-line-between-statements': 'off',
    'newline-before-return': 'off',
    'no-extend-native': 'off',
    'no-useless-escape': 'off'
  }
}
/* eslint-disable */
