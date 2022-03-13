<template>
  <v-list nav dense>
    <div v-for="(item, index) in menu" :key="index">
      <div v-if="(item.key || item.text) && checkMyRole(item) " class="pa-1 mt-2 overline">{{ item.key ? $t(item.key) : item.text }}</div>
      <nav-menu :menu="item.items" />
    </div>
  </v-list>
</template>

<script>
import NavMenu from './NavMenu.vue'

export default {
  components: {
    NavMenu
  },
  props: {
    menu: {
      type: Array,
      default: () => []
    }
  },
  methods: {
    checkMyRole(v) {
      if (v.role)
        return v.role.includes(this.$store.getters['auth/getUserInfo'].Role) // Verify each item in Nav list with role of user to show up

      return false
    }
  }
}
</script>
