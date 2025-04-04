<script setup lang="ts">
import { ref } from 'vue';

const email = ref('');
const users = ref<string[]>([]);

const inviteUser = () => {
  if (email.value) {
    users.value.push(email.value);
    email.value = '';
  }
};

const removeUser = (user: string) => {
  users.value = users.value.filter(u => u !== user);
};
</script>

<template>
  <div>
    <h2>Manage Users</h2>
    <input v-model="email" placeholder="Enter user email" />
    <button @click="inviteUser">Invite</button>
    <ul>
      <li v-for="user in users" :key="user">
        {{ user }}
        <button @click="removeUser(user)">Remove</button>
      </li>
    </ul>
  </div>
</template>

<style scoped>
h2 {
  color: var(--color-primary);
  margin-bottom: 0.5rem;
}


</style>
