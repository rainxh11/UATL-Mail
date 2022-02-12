import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

// GET Api for { Study }
const getAllStudy = async(token, filter) => {
  return await axios.get('/studies' + filter, headerAuth(token))
}

const getOneStudy = async(id, token) => {
  return await axios.get('/studies/' + id, headerAuth(token))
}

const createStudies = async(studies, token) => {
  return await axios.post('/studies/createMany', studies, headerAuth(token))
}

const createStudy = async(studyInfo, token) => {
  if (studyInfo.statusPayment !== undefined) {
    studyInfo.paidAt = Date.now()
  }

  return await axios.post('/studies/', studyInfo, headerAuth(token))
}

const updateStudy = async(id, studyInfo, token) => {
  if (studyInfo.statusPayment !== undefined) {
    studyInfo.paidAt = Date.now()
  }

  return await axios.patch('/studies/' + id, studyInfo, headerAuth(token))
}

const updateStudyProduct = async(id, studyInfo, token) => {
  if (studyInfo.statusPayment !== undefined) {
    studyInfo.paidAt = Date.now()
  }

  return await axios.patch('/studies/' + id + '/product', studyInfo, headerAuth(token))
}

const updateGroup = async(groupInfo, token) => {
  if (groupInfo.statusPayment !== undefined) {
    groupInfo.paidAt = Date.now()
  }

  return await axios.patch('/studies/group/pay', groupInfo, headerAuth(token))
}

const getStudiesCount = async(token) => {
  return await axios.get('/studies/count', headerAuth(token))
}

const getStudiesPaymentStat = async(token) => {
  return await axios.get('/studies/stats/payment', headerAuth(token))
}

const getStudiesPaymentStatInterval = async(start, end, token) => {
  let interval = { startingDate: start, endDate: end }

  if (start > end) {
    interval = { startingDate: end, endDate: start }
  }

  return await axios.post('/studies/stats/payment/interval', interval, headerAuth(token))
}

const getStudiesPaymentGroupByDate = async(groupPeriod = 'day',token) => {
  return await axios.get('/studies/stats/payment/groupbydate?groupPeriod=' + groupPeriod, headerAuth(token))
}

const getStudiesPaymentIntervalGroupByDate = async(start, end, token) => {
  let interval = { startingDate: start, endDate: end }

  if (start > end) {
    interval = { startingDate: end, endDate: start }
  }

  return await axios.post('/studies/stats/payment/interval/groupbydate', interval, headerAuth(token))
}

const updateStudyDebt = async(debtInfo, token) => {
  return await axios.patch('/studies/payDebt', debtInfo, headerAuth(token))
}

const printReceipt = async(id, receiptInfo, token) => {
  return await axios.post('/studies/' + id + '/printreceipt', receiptInfo, headerAuth(token))
}

const cancelStudies = async(ids, token) => {
  return await axios.post('/studies/cancel', ids, headerAuth(token))
}

const searchStudies = async(search, query, token) => {
  return await axios.post('/studies/search' + query, search, headerAuth(token))
}

const printLargeReceipt = async(id, receiptInfo, token) => {
  return await axios.post('/studies/' + id + '/printlargereceipt', receiptInfo, headerAuth(token))
}

const getUserStatIntervalGroupByDate = async(start, end, format, token) => {
  let interval = { startingDate: start, endDate: end, format: format }

  if (start > end) {
    interval = { startingDate: end, endDate: start, format: format }
  }

  return await axios.post('/studies/stats/users/interval/groupbydate', interval, headerAuth(token))
}

const getUserStatAllTimeGroupByDate = async(start, end, format, token) => {
  let interval = { startingDate: start, endDate: end, format: format }

  if (start > end) {
    interval = { startingDate: end, endDate: start, format: format }
  }

  return await axios.post('/studies/stats/users/groupbydate', interval, headerAuth(token))
}

const getUserStatInterval = async(start, end, token) => {
  let interval = { startingDate: start, endDate: end }

  if (start > end) {
    interval = { startingDate: end, endDate: start }
  }

  return await axios.post('/studies/stats/users/interval/', interval, headerAuth(token))
}

const getUserStatAllTime = async(token) => {
  return await axios.get('/studies/stats/users/', headerAuth(token))
}

const getExamStatInterval = async(start, end, token) => {
  let interval = { startingDate: start, endDate: end }

  if (start > end) {
    interval = { startingDate: end, endDate: start }
  }

  return await axios.post('/studies/stats/exams/interval/', interval, headerAuth(token))
}

const getExamStatAllTime = async(token) => {
  return await axios.get('/studies/stats/exams/', headerAuth(token))
}

const getUsersStatFilterByUser = async(userId, statusPayment, start, end, token) => {
  let interval = {
    startingDate: start,
    endDate: end ,
    filter: statusPayment
  }

  if (start > end) {
    interval = { startingDate: end, endDate: start, filter: statusPayment }
  }

  return await axios.post('/studies/stats/payment/interval/filterbyuser/' + userId, interval, headerAuth(token))
}

export {
  getStudiesPaymentGroupByDate,
  getStudiesPaymentStatInterval,
  getAllStudy,
  getOneStudy,
  createStudy,
  getStudiesCount,
  updateStudy,
  getStudiesPaymentStat,
  getStudiesPaymentIntervalGroupByDate,
  updateStudyDebt,
  printReceipt,
  createStudies,
  updateGroup,
  updateStudyProduct,
  getUserStatAllTime,
  getUserStatAllTimeGroupByDate,
  getUserStatInterval,
  getUserStatIntervalGroupByDate,
  getExamStatAllTime,
  getExamStatInterval,
  printLargeReceipt,
  cancelStudies,
  searchStudies,
  getUsersStatFilterByUser
}
