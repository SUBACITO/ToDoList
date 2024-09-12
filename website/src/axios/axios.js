import axios from "axios";

let auth = localStorage.getItem('auth');
if (auth == '') location = 'http://localhost:3000/login';

const instance = axios.create({
    baseURL: 'http://localhost:5075',
    headers: { Authorization: 'Bearer ' + auth}
});


instance.interceptors.request.use(function(config){
    return config;
}, function(error){
    return Promise.reject(error);
});

instance.interceptors.response.use(function(response){
    return response;
}, function(error){
    return Promise.reject(error);
});

export default instance