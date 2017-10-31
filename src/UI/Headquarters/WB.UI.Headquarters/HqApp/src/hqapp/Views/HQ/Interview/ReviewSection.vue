<template>
    <div>
        <Section  />
    </div>
</template>
<script>
export default {
  mounted() {
    this.$store.dispatch("getFlags");
  }
};
</script>
