<template>
  <div>
    <h1>Hello</h1>
    <router-view></router-view>
  </div>
</template>
<script lang='ts'>
  export default {
    name: 'app'
    // $hq-style-path: "../../WB.UI.Headquarters/Dependencies/css/_mixins.scss"
  }
</script>
<style lang='scss'>
  $icon-font-path: "~bootstrap-sass/assets/fonts/bootstrap/";

  @import "../../WB.UI.Headquarters/Dependencies/css/_variables.scss";
  @import "~bootstrap-sass/assets/stylesheets/_bootstrap.scss";
  @import "../../WB.UI.Headquarters/Dependencies/css/common-markup.scss";
</style>
