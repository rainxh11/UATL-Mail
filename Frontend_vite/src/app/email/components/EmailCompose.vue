<template>
  <v-dialog
    ref="composeDialog"
    v-model="dialog"
    :fullscreen="fullscreen || $vuetify.breakpoint.mdAndDown"
    :hide-overlay="!$vuetify.breakpoint.mdAndDown"
    persistent
    width="600"
    content-class="dialog-bottom"
    @keydown.esc="$emit('close-dialog')"
  >
    <v-card>
      <v-card-title v-if="hideButtons" class="pa-2">
        {{ $t('email.sending') }}...
      </v-card-title>
      <v-card-title v-if="!hideButtons" class="pa-2">
        {{ $t('email.compose') }}
        <v-spacer></v-spacer>
        <v-btn v-if="!$vuetify.breakpoint.mdAndDown" icon @click="fullscreen = !fullscreen; minimized = false;">
          <v-icon>{{ fullscreen ? 'fa-down-left-and-up-right-to-center' : 'fa-up-right-and-down-left-from-center' }}</v-icon>
        </v-btn>
        <v-btn icon @click="minimized = !minimized; fullscreen = false;">
          <v-icon>{{ minimized ? 'fa-regular fa-window-maximize' : 'fa-window-minimize' }}</v-icon>
        </v-btn>
        <v-btn icon @click="$emit('close-dialog')">
          <v-icon>fa-xmark</v-icon>
        </v-btn>
      </v-card-title>

      <v-divider v-if="!hideButtons" />

      <email-editor v-if="!minimized" @upload="isUploading($event)"></email-editor>
    </v-card>
  </v-dialog>
</template>

<script>
import EmailEditor from './EmailEditor.vue'

/*
|---------------------------------------------------------------------
| Email Compose Component
|---------------------------------------------------------------------
|
| Compose dialog to wrap the email editor
|
*/
export default {
  components: {
    EmailEditor
  },
  props: {
    // Show compose dialog
    showCompose: {
      type: Boolean,
      default: false
    }
  },
  data () {
    return {
      dialog: false,
      fullscreen: false,
      minimized: false,
      hideButtons: false
    }
  },
  watch: {
    showCompose(val) {
      this.dialog = val
    },
    dialog() {
      this.$nextTick(this.$refs.composeDialog.showScroll)
    }
  },
  mounted() {
    this.dialog = this.showCompose
  },
  methods: {
    isUploading(event) {
      this.hideButtons = event
    }
  }
}
</script>

<style lang="scss">
.dialog-bottom {
  position: fixed;
  bottom: 10px;
  right: 10px;
}
</style>
