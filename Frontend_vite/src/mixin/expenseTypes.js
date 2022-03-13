export const expenseTypes = {
  data () {
    return {
      expenseTypes : [
        {
          text: this.$t('expenseType.productPurchase'),
          type: 'productPurchase',
          color: 'blue'
        },
        {
          text: this.$t('expenseType.employeeSalary'),
          type: 'employeeSalary',
          color: 'green'
        },
        {
          text: this.$t('expenseType.taxPayment'),
          type: 'taxPayment',
          color: 'purple'
        },
        {
          text: this.$t('expenseType.clientRefund'),
          type: 'clientRefund',
          color: 'orange'
        },
        {
          text: this.$t('expenseType.moneyWithdraw'),
          type: 'moneyWithdraw',
          color: 'grey'
        },
        {
          text: this.$t('expenseType.billPayment'),
          type: 'billPayment',
          color: 'cyan'
        },
        {
          text: this.$t('expenseType.rentPayment'),
          type: 'rentPayment',
          color: '#FF1493'
        },
        {
          text: this.$t('expenseType.missingOrStolen'),
          type: 'missingOrStolen',
          color: 'red'
        },
        {
          text: this.$t('expenseType.damagedItem'),
          type: 'damagedItem',
          color: 'yellow'
        },
        {
          text: this.$t('expenseType.otherExpense'),
          type: 'otherExpense',
          color: 'black'
        }]
    }
  }
}
