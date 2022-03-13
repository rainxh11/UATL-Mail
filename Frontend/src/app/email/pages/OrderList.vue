<template>
  <v-card class="d-flex flex-grow-1 flex-column" :loading="loading">
    <v-card-title >
      <v-row no-gutters dense>
        <v-col
          cols="12"
          lg="8"
          md="11"
          sm="11"
          xs="11"
        >
          <v-text-field
            append-icon="mdi-magnify"
            class="flex-grow-1 mr-md-2"
            outlined
            dense
            hide-details
            clearable
            :placeholder="$t('common.search')"
          ></v-text-field>
        </v-col>
        <v-col cols="1">
          <v-btn
            v-show="selected.length === 0"
            icon
            :loading="loading"
            @click="page = 1; refresh({ page: page, limit: pageSize })"
          >
            <v-icon>mdi-refresh</v-icon>
          </v-btn>
        </v-col>
        <v-col
          cols="12"
          lg="3"
          md="12"
          sm="12"
        >
          <v-pagination
            v-model="page"
            circle
            dense
            color="primary"
            :length="orders.PageCount"
            :total-visible="5"
            @input="refresh({ page: page, limit: pageSize })"
          ></v-pagination>
        </v-col>
      </v-row>
    </v-card-title>
    <v-divider/>
    <v-list
      v-for="group in orders.Data
        .AsEnumerable()
        .GroupBy(x => new Date(x.SentOn ? x.SentOn : x.CreatedOn) .toString('yyyy/MM/dd'), x => x)
        .ToArray()"
      :key="group"
      class="px-2"
    >
      <v-subheader>
        <v-list-item-action class="d-flex flex-row align-center">
          <v-list-item-action class="d-flex flex-row align-center">
            <v-icon>fa-regular fa-calendar-day</v-icon>
          </v-list-item-action>
          <v-list-item-action-text>
            <span class="text--primary text-h6">
              {{ new Date(group.key) | formatDate('dddd, DD MMMM yyyy') | uppercase }}
            </span>
          </v-list-item-action-text>
        </v-list-item-action></v-subheader>
      <v-divider/>
      <template v-for="(item, index) in group.values()">
        <v-list-item
          :key="item.ID"
          two-line
          link
          :class="{
            'v-list-item--active warning--text': item.Reviewed && !item.Approved,
            'v-list-item--active success--text': item.Approved,
            'v-list-item--active primary--text': selected.indexOf(item.ID) !== -1
          }"
          @click.stop
        >

          <v-list-item-avatar v-if="item.From" class="d-flex flex-row ma-1">
            <v-img :src="getAvatar(item.From.ID)" lazy-src="/images/avatars/generic.jpg"/>
          </v-list-item-avatar>        
          <v-list-item-avatar v-if="item.To" class="d-flex flex-row ma-1">
            <v-img :src="getAvatar(item.To.ID)" lazy-src="/images/avatars/generic.jpg"/>
          </v-list-item-avatar>

          <v-list-item-action class="align-rigth d-flex flex-row" >
            <v-list-item-action-text v-if="item.From" class="px-1 text-lg-body-1">
              {{ $t('email.from') }}<b>{{ item.From.Name }}</b>
              <span v-if="item.From.Description" class="px-1">({{ item.From.Description }})</span>
            </v-list-item-action-text>
            <v-list-item-action-text v-if="item.To" class="px-1 text-lg-body-1">
              {{ $t('email.to') }}<b>{{ item.To.Name }}</b>
              <span v-if="item.To.Description" class="px-1">({{ item.To.Description }})</span>
            </v-list-item-action-text>
          </v-list-item-action>

          <v-list-item-content class="pa-2 align-rigth d-flex flex-row">
            <v-list-item-title class="text-h6 black--text">{{ item.Subject }}</v-list-item-title>
          </v-list-item-content>
          <v-list-item-action>
            <v-btn v-if="!item.Approved" rounded class="success" @click="approve(item)">
              {{ $t('email.approve') }}
            </v-btn>
            <v-chip
              v-else
              outlined
              color="success"
            >
              {{ $t('email.approved') }}
              <v-icon small class="px-1">fa-check</v-icon>
            </v-chip>
          </v-list-item-action>

          <v-list-item-action>
            <v-list-item-action-text>
              <v-tooltip bottom>
                <template v-slot:activator="{ on, attrs }">
                  <span v-bind="attrs" class="text--primary text-lg-body-2" v-on="on">
                    {{ item.SentOn | formatTimeAgo }}
                    <v-icon color="primary" x-small>fa-regular fa-clock</v-icon>
                  </span>
                </template>
                <span >{{ item.SentOn | formatDate('HH:mm | DD MMMM YYYY') }}</span>
              </v-tooltip>
            </v-list-item-action-text>
          </v-list-item-action>
        </v-list-item>

        <v-divider
          v-if="index + 1 < orders.Data.length > 0"
          :key="index"
        ></v-divider>
      </template>
      <template v-if="orders.Data.length === 0">
        <div class="px-1 py-6 text-center">{{ $t('email.emptyList') }}</div>
      </template>
    </v-list>
  </v-card>
</template>

<script>
import { getAllOrders, searchOrders, reviewOrder, approveOrder } from '@/api/orders'

export default {

  data() {
    return {
      loading: false,
      search: '',
      selected: [],
      headers: [
        { text: this.$t('email.headers.subject'), align: 'left', value: 'Subject' },
        { text: this.$t('email.headers.sender'), align: 'left', value: 'From' },
        { text: this.$t('email.headers.destination'), align: 'left', value: 'To' },
        { text: this.$t('email.headers.status'), value: 'Approved' },
        { text: this.$t('email.headers.sentOn'), value: 'SentOn' },
        { text: '', sortable: false, align: 'right', value: 'action' }
      ],
      orders: [],
      starred: [],
      page:1,
      pageSize: 10
    }
  },
  watch: {
    page(val) {
      this.refresh()
    }
  },
  async created() {
    this.$mailHub.on('refresh_orders', (x) => {
      if (this.search.length > 0) {
        this.refresh()
      }
    })
    try {
      if (this.$mailHub.state === 'Disconnected') await this.$mailHub.start()
    }
    catch (err) {
      console.log(err)
    }
  },
  mounted() {
    this.refresh()
  },
  methods: {
    refresh(params =  { page: this.page, limit: this.pageSize }) {
      if (this.search.length > 0) {
        this.searchOrders(search, params)
      } else {
        this.getOrders(params)
      }
    },
    getAvatar(id) {
      return `${this.$apiHost}/api/v1/account/${id}/avatar`
    },
    getOrders(params) {
      this.loading = true
      getAllOrders(params)
        .then(res => {
          this.orders = res.data
          console.log(res.data)
        })
        .catch(err => console.log(err))
        .finally(() => this.loading = false)
    },
    searchOrders(search, params) {
      this.loading = true
      searchOrders(search, params)
        .then(res => {
          this.orders = res.data
        })
        .catch(err => console.log(err))
        .finally(() => this.loading = false)
    },
    approve(item) {
      approveOrder(item.ID)
        .then(res => this.refresh())
        .catch(err => console.log(err))
    }
  }
}
</script>
