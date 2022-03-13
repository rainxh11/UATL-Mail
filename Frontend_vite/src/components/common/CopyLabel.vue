<template>
  <v-tooltip bottom>
    <template v-slot:activator="{ on }">
      <div ref="copylabel" class="copylabel animate__faster" v-on="on" @click.stop.prevent="copy">{{ text }}</div>
    </template>
    <span>{{ tooltip }}</span>
  </v-tooltip>
</template>

<script>
/*
|---------------------------------------------------------------------
| Copy Label Component
|---------------------------------------------------------------------
|
| Copy to clipboard with the plugin clipboard `this.$clipboard`
|
*/
export default {
  props: {
    // Text to copy to clipboard
    text: {
      type: String,
      default: ''
    },
    // Text to show on toast
    toastText: {
      type: String,
      default: 'Copié'
    },
    /**
     * CSS animation with animate.css
     * https://animate.style/
     */
    animation: {
      type: String,
      default: 'heartBeat'
    }
  },
  data() {
    return {
      tooltip: this.$t('common.copy'),
      defaultTooltip : this.$t('common.copy'),
      timeout: null
    }
  },
  beforeDestroy() {
    if (this.timeout) clearTimeout(this.timeout)
  },
  methods: {
    copy() {
      this.$animate(this.$refs.copylabel, this.animation)
      this.$clipboard(this.text, this.toastText)
      this.tooltip = this.$t('common.copied')

      this.timeout = setTimeout(() => {
        this.tooltip = this.defaultTooltip
      }, 2000)
    }
  }
}
</script>

<style scoped>
.copylabel {
  cursor: pointer;
  display: inline-block;
  border-bottom: 1px dashed;
}
</style>
