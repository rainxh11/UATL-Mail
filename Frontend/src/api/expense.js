import axios from '@/api'

function headerAuth(token) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'content-Type': 'Application/json'
    }
  }
}

const getExpense = async (id, token) => {
  return await axios.get('/expenses/' + id, headerAuth(token) )  }

const getAllExpense = async (token, filter = null) => {
  return await axios.get('/expenses' + filter, headerAuth(token) )  }

const createExpense = async (expense, token) => {
  return await axios.post('/expenses', expense, headerAuth(token) )  }

const updateExpense = async (id, expense, token) => {
  return await axios.patch('/expenses/' + id, expense, headerAuth(token) )  }

const deleteExpense = async (id, token) => {
  return await axios.delete('/expenses/' + id, headerAuth(token) )  }

const getExpenseSum = async (token) => {
  return await axios.get('/expenses/sum', headerAuth(token) )  }

const getExpenseStats = async (token) => {
  return await axios.get('/expenses/stats', headerAuth(token) )  }

export {
  getExpenseStats, getExpense, getAllExpense, updateExpense, deleteExpense, createExpense, getExpenseSum
}
