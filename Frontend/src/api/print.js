import axios from '@/api'
import documentServer from './axios_documentserver'

function headerAuth(token,type) {
  return {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'multipart/form-data'
    }
  }
}

const printStudy = async (report, token) => {
  return await axios.post('/printStudy', report , headerAuth(token,'multipart/form-data') )
}

const printInvoice = async(risReportServer, invoice) => {
  return await documentServer.post(risReportServer + 'printreceipt', invoice)
}
const printDebtInvoice = async(risReportServer, invoice) => {
  return await documentServer.post(risReportServer + 'printdebtreceipt', invoice)
}

const printDicom = async(printDicomDocumentServer, report) => {
  return await documentServer.post(printDicomDocumentServer, report)
}

export {
  printStudy, printDicom, printInvoice, printDebtInvoice
}
