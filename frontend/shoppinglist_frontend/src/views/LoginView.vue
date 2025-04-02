<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const username = ref('')
const password = ref('')

const login = () => {
  authStore.login(username.value, password.value)
  if (authStore.isAuthenticated) {
    router.push('/')
  }
}
</script>

<template>
  <div class="login">
    <h1>Login</h1>
    <input v-model="username" placeholder="Username" />
    <input v-model="password" type="password" placeholder="Password" />
    <button @click="login">Login</button>
  </div>
</template>

<style scoped>
.login {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start; /* Ändern Sie dies von center zu flex-start */
  height: 100vh;
  margin-top: 2rem; /* Fügen Sie dies hinzu, um den Abstand von oben zu erhöhen */
}

input {
  margin: 0.5em 0;
  padding: 0.5em;
  font-size: 16px;
}

button {
  padding: 10px 20px;
  font-size: 16px;
  cursor: pointer;
}
</style>
