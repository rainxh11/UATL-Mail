export const initTable = {
  data () {
    return {
      searchQuery: '',
      isLoadingRefreshBtn: false,
      isloadingTable: false,
      itemTable: [],
      selectedItem: [],
      itemsCount: 0,
      pageInfo: 1,
      pageCountInfo: 1
    }
  }
}

export const roles = {
  data () {
    return {
      userRoles : [
        {
          roleName : this.$t('setting.roles.admin'),
          role : 'admin'
        },
        {
          roleName : this.$t('setting.roles.user'),
          role : 'user'
        }]
    }
  }
}
