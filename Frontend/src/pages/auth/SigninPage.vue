<template>
  <div >
    <v-card class="text-center pa-1">
      <v-card-title class="justify-center display-1 mb-2">{{ this.$t('signin.welcome') }}</v-card-title>
      <v-card-subtitle>{{ this.$t('signin.signinSubheader') }}</v-card-subtitle>

      <!-- sign in form -->
      <v-card-text>
        <v-form ref="form" v-model="isFormValid" lazy-validation>
          <v-text-field
            v-model="email"
            :rules="[rules.required]"
            :validate-on-blur="false"
            :error="error"
            :label="$t('login.username')"
            name="email"
            outlined
            @keyup.enter="submit"
            @change="resetErrors"
          ></v-text-field>

          <v-text-field
            v-model="password"
            :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
            :rules="[rules.required]"
            :type="showPassword ? 'text' : 'password'"
            :error="error"
            :error-messages="errorMessages"
            :label="$t('login.password')"
            name="password"
            outlined
            @change="resetErrors"
            @keyup.enter="submit"
            @click:append="showPassword = !showPassword"
          ></v-text-field>

          <v-btn
            :loading="isLoading"
            :disabled="isSignInDisabled"
            block
            x-large
            color="primary"
            class="text-uppercase text-lg-h6"
            @click="submit"
          >
            <v-icon class="pa-1">mdi-login-variant</v-icon>
            {{ $t('login.button') }} </v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </div>

</template>

<script>

import { signIn } from '@/api/auth'
import { mapActions, mapGetters, mapMutations } from 'vuex'
import Vuecookie from 'vue-cookies'
/*
|---------------------------------------------------------------------
| Sign In Page Component
|---------------------------------------------------------------------
|
| Sign in template for user authentication into the application
|
*/
export default {
  data() {
    return {
      // sign in buttons
      isLoading: false,
      isSignInDisabled: false,

      // form
      isFormValid: true,
      email: '',
      password: '',

      // form error
      error: false,
      errorMessages: '',

      errorProvider: false,
      errorProviderMessages: '',

      // show password field
      showPassword: false,

      // input rules
      rules: {
        required: (value) => (value && Boolean(value)) || 'Required'
      }
    }
  },
  methods: {
    ...mapMutations('auth',['setToken','setIsAuth']),
    ...mapActions('app',['showSuccess','showError']),
    ...mapActions('auth',['retriveToken']),
    submit() {
      if (this.$refs.form.validate()) {
        this.isLoading = true
        this.isSignInDisabled = true
        this.signIn(this.email, this.password)
      }
    },
    signIn(email, password) {
      signIn(email, password).then((res) => {
        if (res.data.status === 'success') {
          this.retriveToken({ token : res.data.token, userInfo: res.data.data.user })
          Vuecookie.set('T', res.data.token)
          this.showSuccess('Success')
          this.$router.push('/')
        }

        // eslint-disable-next-line handle-callback-err
      }).catch( (err) => {
        this.showError(err)
        this.error = true
        this.errorMessages = err.toString()
        this.isLoading = false
        this.isSignInDisabled = false
      })
    },
    resetErrors() {
      this.error = false
      this.errorMessages = ''

    }
  }
}
</script>
