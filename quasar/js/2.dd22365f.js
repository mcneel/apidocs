(window["webpackJsonp"]=window["webpackJsonp"]||[]).push([[2],{"8b24":function(e,t,s){"use strict";s.r(t);var n=function(){var e=this,t=e.$createElement,s=e._self._c||t;return s("q-page",[s("h4",[e._v(e._s(e.title))]),s("i",[e._v("Namespace: "),s("a",{attrs:{href:"#"},on:{click:function(t){return e.setSelectedItem(e.namespace)}}},[e._v(e._s(e.namespace))])]),s("p",[e._v(e._s(e.vm.summary))]),s("q-list",{staticClass:"rounded-borders q-mt-md",attrs:{bordered:""}},e._l(e.memberSections,(function(t){return s("q-expansion-item",{key:t.title,attrs:{"expand-separator":"","switch-toggle-side":"",dense:"","dense-toggle":"",value:t.expanded,label:t.title,"content-inset-level":1}},e._l(t.items,(function(t){return s("q-item",{key:t.signature},[s("q-item-section",[s("q-item-label",[e._v(e._s(t.signature))]),s("q-item-label",{attrs:{caption:""}},[t.since?s("q-badge",{attrs:{outline:"",color:"secondary"}},[e._v(e._s(t.since.toFixed(1))+"\n            "),s("q-tooltip",[e._v("Available since "+e._s(t.since.toFixed(1)))])],1):e._e(),e._v("\n          "+e._s(t.summary)+"\n        ")],1)],1)],1)})),1)})),1)],1)},a=[],i=(s("7f7f"),s("185a")),o={data:function(){return{vm:{},title:"",namespace:"",memberSections:[]}},created:function(){i["a"].setSelectedItemChangedCallback(this.onChangeSelectedItem)},mounted:function(){this.$route.params&&this.$route.params.datatype&&i["a"].setSelectedItem(this.$route.params.datatype)},methods:{setSelectedItem:function(e){this.$router.push("/"+e),i["a"].setSelectedItem(e)},onChangeSelectedItem:function(e){console.log("selected item changed to "+e),this.vm=e;var t=e.name.lastIndexOf(".");this.title=e.name.substring(t+1),this.namespace=e.name.substring(0,t),this.memberSections=[],e.constructors&&this.memberSections.push({title:"Constructors ("+e.constructors.length+")",items:e.constructors,expanded:!0}),e.properties&&this.memberSections.push({title:"Properties ("+e.properties.length+")",items:e.properties,expanded:!0}),e.methods&&this.memberSections.push({title:"Methods ("+e.methods.length+")",items:e.methods,expanded:!0}),e.events&&this.memberSections.push({title:"Events ("+e.events.length+")",items:e.events,expanded:!0})}}},r=o,c=s("2877"),m=s("eebe"),l=s.n(m),d=s("9989"),u=s("1c1c"),p=s("3b73"),h=s("66e5"),b=s("4074"),g=s("0170"),v=s("58a8"),_=s("05c0"),f=Object(c["a"])(r,n,a,!1,null,null,null);t["default"]=f.exports;l()(f,"components",{QPage:d["a"],QList:u["a"],QExpansionItem:p["a"],QItem:h["a"],QItemSection:b["a"],QItemLabel:g["a"],QBadge:v["a"],QTooltip:_["a"]})}}]);