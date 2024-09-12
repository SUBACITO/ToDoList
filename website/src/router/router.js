import { createRouter, createWebHistory } from "vue-router";
import LoginComponent from '../components/Login.vue';
import HomeComponent from '../components/Home.vue';

const routes = [
  {
    path: '/home',
    name: 'HomeComponent',
    component : HomeComponent,
    meta : {
      requiresAuth : true
    }
  },
  {
    path: '/login',
    name: 'LoginComponent',
    component : LoginComponent,
  },
  {
    path : '/:pathMatch(.*)*',
    redirect : '/login'
  }
];

const router = createRouter({
  history : createWebHistory(),
  routes,
})


router.beforeEach((to, from, next) =>{
  const IsLogin = localStorage.getItem('auth');

  if(to.meta.requiresAuth && !IsLogin)
  {
    next('/login');
  }
  else{
    next();
  }
});

export default router;
