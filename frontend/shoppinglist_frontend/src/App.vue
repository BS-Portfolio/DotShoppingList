<script setup lang="ts">
import {RouterLink, RouterView, useRouter} from 'vue-router'
import {computed} from 'vue'
import {useAuthStore} from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()

const isAuthenticated = computed(() => authStore.isAuthenticated)

const logout = () => {
  authStore.logout()
  authStore.$reset()
  router.push('/login')
}
</script>

<template>
  <header>
    <img alt="Shopping List logo" class="logo" src="@/assets/logo.png" width="125" height="125"/>
    <div class="wrapper">
      <nav>
        <RouterLink v-if="isAuthenticated" to="/dashboard">Dashboard</RouterLink>
        <button v-if="isAuthenticated" @click="logout" class="logout-button">Logout</button>
      </nav>
    </div>
  </header>

  <RouterView/>
</template>

<style scoped>
header {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  line-height: 1.5;
  max-height: 100vh;
}

.logo {
  display: block;
  margin: auto;
}

nav {
  width: 100%;
  font-size: 12px;
  text-align: center;
  margin-top: 1rem;
}

nav a {
  display: inline-block;
  padding: 0 1rem;
  border-left: 1px solid var(--color-border);
}

nav a:first-of-type {
  border: 0;
}

.logout-button {
  margin-left: 1rem;
  padding: 5px 10px;
  font-size: 16px;
  cursor: pointer;
}

@media (min-width: 1024px) {
  header {
    flex-direction: column;
    place-items: center;
  }

  .logo {
    margin: 0;
  }

  header .wrapper {
    display: flex;
    place-items: center;
    flex-wrap: wrap;
  }

  nav {
    text-align: center;
    margin-left: 0;
    font-size: 1rem;
    padding: 1rem 0;
    margin-top: 1rem;
  }
}
</style>
