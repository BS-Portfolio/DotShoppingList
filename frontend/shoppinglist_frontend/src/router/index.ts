import {createRouter, createWebHistory} from 'vue-router'
import ShoppingListView from '../views/ShoppingListView.vue'
import LoginView from '../views/LoginView.vue'
import {useAuthStore} from '@/stores/auth'
import DashboardView from "@/views/DashboardView.vue";
import RegistrationView from "@/views/RegistrationView.vue";
import ShoppingList from "@/components/ShoppingList.vue";
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'Shopping List',
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
      component: DashboardView,
      meta: {requiresAuth: true},
    },
    {
      path: '/registration',
      name: 'registration',
      component: RegistrationView
    },
    {
      path: '/shopping-list',
      name: 'shopping-list',
      component: ShoppingList,
      meta: {requiresAuth: true},
    },
  ],
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true';

  if (to.meta.requiresAuth && !authStore.isAuthenticated && !isAuthenticated) {
    next({ name: 'login' });
  } else if (to.path === '/shopping-list' && !to.query.listId) {
    next({ name: 'dashboard' });
  } else {
    if (isAuthenticated) {
      authStore.isAuthenticated = true;
    }
    next();
  }
});

export default router
