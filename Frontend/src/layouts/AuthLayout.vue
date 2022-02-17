<template>
  <div class="d-flex text-center flex-column flex-md-row flex-grow-1">

    <v-col >

      <div class="pa-md-2 flex-grow-1 flex-grow-1 align-center d-flex flex-column">
        <img style="width: 20%; alignment: center" src="/images/logo/header.svg" />

        <div class="layout-content  w-full pa-3">
          <slot></slot>
        </div>
        <v-spacer></v-spacer>

      </div>

    </v-col>

  </div>
</template>

<script>
import { mapState } from 'vuex'

export default {
  computed: {
    ...mapState('app', ['product'])
  }
}
</script>

<style scoped>
.layout-side {
  max-width: 420px;
}

.layout-content {
  max-width: 480px;
}
</style>
