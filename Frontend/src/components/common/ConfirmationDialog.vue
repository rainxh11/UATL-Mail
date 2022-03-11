<template>
  <v-dialog
    v-model="show"
    :max-width="width"
    class="elevation-24"
    @keydown.esc="cancel"
    @keydown.enter.prevent="agree"
  >
    <v-card>
      <v-toolbar dark dense flat :color="titleColor">
        <v-toolbar-title class="align-center pa-1 text--right text-lg-h6 font-weight-bold d-flex">
          <v-icon class="px-1" :color="iconColor">{{ icon }}</v-icon> {{ title }}
        </v-toolbar-title>
      </v-toolbar>
      <v-card-text
        v-show="!!message"
        class="pa-4 text-lg-body-1 font-weight-bold "
        v-html="message"
      ></v-card-text>
      <v-card-actions class="pt-3">
        <v-spacer></v-spacer>
        <v-btn
          v-if="!noconfirm"
          color="grey"
          text
          x-large
          class="text-h6 font-weight-bold"
          @click.native="cancel"
        >
          {{ this.$t('common.cancel') }}
        </v-btn>
        <v-btn
          :color="titleColor"
          x-large
          outlined
          class="text-h6 font-weight-bold mx-2"
          @click.native="agree"
        >
          {{ this.$t('common.ok') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
export default {
  name: 'ConfirmationDialog',
  props: {
    width : {
      type : Number,
      default : 400
    },
    noconfirm : {
      type: Boolean,
      default: false
    },
    icon: {
      type: String,
      default:'fa-question-circle'
    },
    iconColor:{
      type: String,
      default: 'primary darken-2'
    },
    titleColor:{
      type: String,
      default: 'primary'
    }
  },
  data() {
    return {
      title : 'Confirmation',
      message : null,
      show: false,
      resolve: null,
      reject: null
    }
  },
  methods: {
    open(message, title) {
      this.show = true
      this.title = title
      this.message = message

      return new Promise((resolve, reject) => {

        this.resolve = resolve
        this.reject = reject
      })
    },
    agree() {
      this.resolve(true)
      this.show = false
    },
    cancel() {
      this.resolve(false)
      this.show = false
    }
  }
}
</script>

<style scoped>

</style>
