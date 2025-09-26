import axios from 'axios'

const api = axios.create({
  baseURL: 'https://localhost:7071',
  withCredentials: false
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('jwtToken')
  if (token) {
    config.headers = config.headers || {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export default api