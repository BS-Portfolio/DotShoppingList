import {createRouter, createWebHistory} from 'vue-router'
import ShoppingListView from '../views/ShoppingListView.vue'
import LoginView from '../views/LoginView.vue'
import {useAuthStore} from '@/stores/auth'
import DashboardView from "@/views/DashboardView.vue";
import RegistrationView from "@/views/RegistrationView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: ShoppingListView,
      meta: {requiresAuth: true},
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: DashboardView
    },
    {
      path: '/registration',
      name: 'registration',
      component: RegistrationView
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../views/AboutView.vue'),
    },
  ],
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true'
  if (to.meta.requiresAuth && !authStore.isAuthenticated && !isAuthenticated) {
    next({name: 'login'})
  } else {
    if (isAuthenticated) {
      authStore.isAuthenticated = true
    }
    next()
  }
})

export default router
