import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { Client }
const getAllClient = async (token, filter) => {
  return await axios.get('/clients' + filter, headerAuth(token) )  }

const getAllClientsAggregate = async (search, query, token) => {
  return await axios.post('/clients/aggregate' + query, search,headerAuth(token) )  }

const getDebClients = async (search, query, token) => {
  return await axios.post('/clients/debt' + query, search,headerAuth(token) )  }

const getOneClient = async (id, token) => {
  return await axios.get('/clients/' + id, headerAuth(token) )  }

const getClientAggregate = async (id, token) => {
  return await axios.get('/clients/aggregate/' + id, headerAuth(token) )  }

const getDebtAmount = async (token) => {
  return await axios.get('/clients/debtStat/', headerAuth(token) )  }

const createClient = async (clientInfo, token) => {
  return await axios.post('/clients/', clientInfo, headerAuth(token) )  }

const payDebt = async (debtInfo, id ,token) => {
  return await axios.post('/clients/' + id + '/payDebt', debtInfo, headerAuth(token) )  }

const updateClient = async (id, client, token) => {
  return await axios.patch('/clients/' + id, client, headerAuth(token))
}

const getClientsCount = async (token) => {
  return await axios.get('/clients/count', headerAuth(token) )  }

const getDebtClientsCount = async (token) => {
  return await axios.get('/clients/debt/count', headerAuth(token) )  }

const getSimilarClients = async (search, limit, token) => {
  return await axios.post('/clients/similar?limit=' + parseFloat(limit), search, headerAuth(token) )  }

const searchClients = async (search, token) => {
  return await axios.post('/clients/search', search, headerAuth(token) )  }

const searchDebtClients = async (search, query, token) => {
  return await axios.post('/clients/debt/search' + query, search, headerAuth(token) )  }

const printReceiptDebt = async (id, receiptInfo, token) => {
  return await axios.post('/clients/' + id + '/printrecieptdebt', receiptInfo ,headerAuth(token) )  }

const getUserStatsFilterByUserClients = async(userId, debtType, start, end, token) => {
  let interval = {
    startingDate: start,
    endDate: end ,
    debtType: debtType
  }

  if (start > end) {
    interval = { startingDate: end, endDate: start, debtType: debtType }
  }

  return await axios.post('/clients/debtStat/interval/filterbyuser/' + userId, interval, headerAuth(token))
}

const getClientsCountStatsInterval = async (start, end, token) => {
  let interval = { startingDate: start, endDate: end }

  if ( start > end) {
    interval = { startingDate: end, endDate: start }
  }

  return await axios.post('/clients/stats/interval/count', interval, headerAuth(token) )
}

const getClientsDemographicStats = async (groupByGenders, token) => {
  return await axios.get('/clients/stats/demographic?genders=' + groupByGenders, headerAuth(token) )
}

const getClientsDemographicStatsInterval = async (start, end, groupByGenders, token) => {
  let interval = { startingDate: start, endDate: end }

  if ( start > end) {
    interval = { startingDate: end, endDate: start }
  }

  return await axios.post('/clients/stats/demographic/interval?genders=' + groupByGenders, interval, headerAuth(token) )
}

export {
  getClientsDemographicStatsInterval,
  getClientsDemographicStats,
  getDebtAmount,
  getDebClients,
  getClientsCountStatsInterval,
  getAllClient ,
  updateClient ,
  getOneClient ,
  createClient ,
  getClientsCount ,
  searchClients ,
  payDebt ,
  printReceiptDebt,
  getDebtClientsCount,
  searchDebtClients,
  getClientAggregate,
  getAllClientsAggregate,
  getUserStatsFilterByUserClients,
  getSimilarClients
}
