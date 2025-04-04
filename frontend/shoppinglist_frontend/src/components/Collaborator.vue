<script setup lang="ts">
import {ref, watch} from 'vue';

interface User {
  email: string;
  status: string;
}

const email = ref('');
const users = ref<User[]>(JSON.parse(sessionStorage.getItem('users') || '[]'));

const inviteUser = () => {
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (email.value && emailPattern.test(email.value)) {
    users.value.push({email: email.value, status: 'pending'});
    email.value = '';
    sessionStorage.setItem('users', JSON.stringify(users.value));
  } else {
    alert('Please enter a valid email address.');
  }
};

const removeUser = (userEmail: string) => {
  users.value = users.value.filter(u => u.email !== userEmail);
  sessionStorage.setItem('users', JSON.stringify(users.value));
};

// Watch for changes in users and update session storage
watch(users, (newUsers) => {
  sessionStorage.setItem('users', JSON.stringify(newUsers));
}, {deep: true});
</script>

<template>
  <div>
    <h2>Manage Users</h2>
  </div>
  <div>
    <input v-model="email" @keyup.enter="inviteUser" placeholder="Enter user email &#9166"/>
    <button class="invite-button" @click="inviteUser">Invite</button>
    <ul>
      <li v-for="user in users" :key="user.email" class="user-item">
        <div class="user-info">
          {{ user.email }} <span :class="{ pending: user.status === 'pending' }">({{
            user.status
          }})</span>
        </div>
        <button class="remove-button" @click="removeUser(user.email)">x</button>
      </li>
    </ul>
  </div>
</template>

<style scoped>
h2 {
  color: var(--color-primary);
  margin-bottom: 0.5rem;
  text-align: center;
}

input {
  width: 90%;
  height: 2vh;
  font-size: 20px;
}

.invite-button {
  width: 9%;
  height: 2.5vh;
  font-size: 20px;
  border-radius: 0;
  border: 0;
  margin: 0;
}

.remove-button {
  color: var(--color-primary);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1rem;
  font-weight: bold;
}

.remove-button:hover {
  background: var(--color-primary);
  color: white;
}

.pending {
  font-weight: bold;
  color: red;
}

.user-item {
  font-size: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.user-info {
  display: flex;
  align-items: center;
}

li {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

ul {
  list-style-type: none;
  padding-left: 0;
}

li:nth-child(even) {
  background-color: #fff8dc;
}
</style>
