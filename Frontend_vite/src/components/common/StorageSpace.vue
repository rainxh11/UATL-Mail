<template>

  <div>
    <v-overlay
      :absolute="true"
      :value="loading"
    >
      <v-progress-circular
        indeterminate
        size="100 "
        width="10"
        color="primary"
      />
    </v-overlay>
    <div class="text-lg-h6 py-1">      <v-icon class="pr-2" x-large>fa-hdd</v-icon>Etat de stockage PACS:</div>
    <v-progress-linear
      rounded
      :value="percentage"
      :height="progressHeight"
      :color="getColor()"
    >
      <div class="text-lg-h5"><strong>{{ Math.round(percentage) }} % - </strong> <i>({{ prettyByte(storage.used) }} / {{ prettyByte(storage.total) }})</i></div>
    </v-progress-linear>
  </div>
</template>

<script>

export default {
  name: 'StorageSpace',
  props: {
    loading: {
      type: Boolean,
      default: false
    },
    color: {
      type: String,
      default: 'green'
    },
    storage:{
      type: Object,
      default: () => {
        return {
          total: 1,
          used: 0
        }
      }
    },
    progressHeight: {
      type: Number,
      default: 35
    }
  },
  computed:{
    percentage() {
      return Math.round(this.storage.used * 100 / this.storage.total)
    }
  },
  methods: {
    getColor() {
      if (this.percentage < 70) {
        return this.color
      } else if (this.percentage >= 70 && this.percentage < 90) {
        return 'orange'
      } else {
        return 'red'
      }
    },
    prettyByte(bytes) {
      const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB']

      if (bytes === 0) return '0 Byte'
      const i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)))

      return parseFloat(bytes / Math.pow(1024, i)).toFixed(2) + ' ' + sizes[i]
    }
  }
}
</script>

<style scoped>

</style>
