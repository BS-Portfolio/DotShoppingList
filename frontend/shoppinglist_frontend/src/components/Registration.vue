<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const firstname = ref('');
const lastname = ref('');
const username = ref('');
const email = ref('');
const password = ref('');

const saveToSession = (key: string, value: any) => localStorage.setItem(key, JSON.stringify(value));

const handleLoginSuccess = () => {
  saveToSession('isAuthenticated', true);
  authStore.isAuthenticated = true;
  router.push('/');
};

const validateEmail = (email: string): boolean => {
  const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return re.test(email);
};

const register = async () => {
  if (!username.value || !email.value || !password.value) {
    alert('Bitte füllen Sie alle Felder aus.');
    return;
  }

  if (!validateEmail(email.value)) {
    alert('Bitte geben Sie eine gültige E-Mail-Adresse ein.');
    return;
  }

  const encodedEmail = btoa(email.value);
  const encodedPassword = btoa(password.value);

  try {
    const response = await fetch('https://localhost:7191/ShoppingListApi/User', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        firstname: firstname.value,
        lastname: lastname.value,
        username: username.value,
        emailAddress64: encodedEmail,
        password64: encodedPassword
      })
    });

    if (!response.ok) {
      const errorText = await response.text();
      if (response.status === 409) {
        throw new Error('Ein Benutzer mit derselben E-Mail-Adresse existiert bereits. Bitte verwenden Sie eine andere Adresse, um sich zu registrieren!');
      }
      throw new Error(errorText);
    }

    const data = await response.json();

    if (data.isAuthenticated) {
      handleLoginSuccess();
    } else {
      alert('Registrierung fehlgeschlagen.');
    }
  } catch (error) {
    console.error('Fehler bei der Registrierung:', error);
    alert(`Ein Fehler ist aufgetreten: ${error.message}`);
  }
};
</script>

<template>
  <div class="login">
    <div class="card">
      <h1 class="caveat-brush-regular">Registrieren</h1>
      <form @submit.prevent="register">
        <input v-model="firstname" placeholder="First name" />
        <input v-model="lastname" placeholder="Last name" />
        <input v-model="email" type="email" placeholder="Email" />
        <input v-model="username" placeholder="Username" />
        <input v-model="password" type="password" placeholder="Password" />
        <button type="submit">Registrieren</button>
      </form>
      <RouterLink to="/login" class="auth-link">Already have an account? Login now.</RouterLink>
    </div>
  </div>
</template>

<style scoped>
@import './../assets/auth.css';
</style>
