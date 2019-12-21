//===========================================
//文件名：mcs_bgyspreadrule.js
//功能描述：合伙人-奖励规则js控制器文件
//创建时间：2019年12月21日 11:33:49;作者：
//===========================================
if (typeof (bgy) === "undefined") {
    bgy= {};
}

if (typeof (bgy.spreadrule) === "undefined") {
    bgy.spreadrule = {};
}

bgy.spreadrule = {
    form: {
        OnLoad: function () {
            var formType = Xrm.Page.ui.getFormType();
            if (formType === 2) {
                var mcs_rulestatus = Xrm.Page.getAttribute("mcs_rulestatus").getValue();//审批状态
                //是否则锁定字段不可编辑
                if (mcs_rulestatus == 0 || mcs_rulestatus == 4) {
                    bgy.spreadrule.utils.DisabledAllControl(false);
                } else {
                    bgy.spreadrule.utils.DisabledAllControl(true);
                }
            }
        },
        //窗体-提交
        Submit: function () {
            var confirmFunc = function () {
                mcc.form.showLoading("系统正在处理中....");
                var amount = 0;
                var entityId = Xrm.Page.data.getEntity().getId();//Id
                var mcs_rulestatus = Xrm.Page.getAttribute("mcs_rulestatus").getValue();//规则状态
                if (entityId == null && entityId.length <= 0) {
                    mcc.form.alert({ title: "消息提示", content: "获取奖励规则失败！", messagetype: mcc.form.messagetype.warning });
                    mcc.form.closeLoading();
                    return false;
                }

                //校验：审批状态是否为草稿或者驳回
                if (mcs_rulestatus != 0 && mcs_rulestatus != 3) {
                    mcc.form.alert({ title: "消息提示", content: "当前的审批状态不符合要求！", messagetype: mcc.form.messagetype.warning });
                    mcc.form.closeLoading();
                    return false;
                }

                //执行提交并更新数据
                bgy.spreadrule.utils.Submit(entityId);
                mcc.form.closeLoading();
            };
            mcc.form.confirm({ title: "消息提示", content: "是否需要对当前记录进行【提交】操作？", confirmcallback: confirmFunc });

        },
        //提交-按钮控制
        SubmitEnable: function () {
            var isEnable = false;
            var formType = Xrm.Page.ui.getFormType();
            if (formType != 2) {
                //非编辑页面不做权限控制
                return isEnable;
            }
            
            var mcs_rulestatus = Xrm.Page.getAttribute("mcs_rulestatus").getValue();//审批状态
            if ((mcs_rulestatus == 0 || mcs_rulestatus == 3)) {
                isEnable = true;
            }
            return isEnable;
        },
        //窗体-审批
        AuditFactory: function () {
            var confirmFunc = function () {
                mcc.form.showLoading("系统正在处理中....");
                var entityId = Xrm.Page.data.getEntity().getId();//采购申请单Id
               
                var mcs_rulestatus = Xrm.Page.getAttribute("mcs_rulestatus").getValue();//审批状态
                if (entityId == null && entityId.length <= 0) {
                    mcc.form.alert({ title: "消息提示", content: "获取奖励规则失败！", messagetype: mcc.form.messagetype.warning });
                    mcc.form.closeLoading();
                    return false;
                }

                //校验：是否为：1(已提交) 审批
                if (mcs_rulestatus != 1) {
                    mcc.form.alert({ title: "消息提示", content: "当前的奖励规则状态不符合要求！", messagetype: mcc.form.messagetype.warning });
                    mcc.form.closeLoading();
                    return false;
                }

                //打开模态窗体
                var parameters = {};
                parameters.orderType = 1;
                var dialogOptions = new Xrm.DialogOptions();
                dialogOptions.height = 400;
                dialogOptions.width = Xrm.Page.context.client.getClient() === "Mobile" ? 400 : 600;
                Xrm.Internal.openDialog("/WebResources/mcs_/Htmls/BGYPOC/BGYSpreadRuleAuditDialog.html", dialogOptions, parameters, null, bgy.spreadrule.utils.AuditFactory);
                mcc.form.closeLoading();
            };
            confirmFunc();
            //mcc.form.confirm({ title: "消息提示", content: "是否需要对当前记录进行【审批】操作？", confirmcallback: confirmFunc });
        },
        //主机厂审批-按钮控制
        AuditFactoryEnable: function () {
            var isEnable = false;
            var formType = Xrm.Page.ui.getFormType();
            if (formType != 2) {
                //非编辑页面不做权限控制
                return isEnable;
            }

            
            var mcs_rulestatus = Xrm.Page.getAttribute("mcs_rulestatus").getValue();//审批状态
            if (mcs_rulestatus == 1) {
                isEnable = true;
            }
            return isEnable;
        },
    },
    utils: {
        Submit: function (entityId) {
            var data = new Object();
            data["mcs_rulestatus"] = 1; //0:草稿;1:已提交
            var id = entityId.replace("{", "").replace("}", "");

            //保存更新
            CRM.Common.updateSync("mcs_spreadrules", id, data, function (result) {
                //更新审批状态成功之后的操作：添加审批记录
                var entity = new Object();
                var userId = Xrm.Page.context.getUserId().replace("{", "").replace("}", "");
                entity["mcs_comments"] = "";//审批意见
                entity["mcs_name"] = bgy.spreadrule.utils.GenOrderNo();//生成订单号
                entity["mcs_approvaldate"] = new Date();//系统当前时间
                entity["mcs_approvaluser@odata.bind"] = "/systemusers(" + userId + ")";//审批人
                entity["mcs_spreadruleid@odata.bind"] = "/mcs_spreadrules(" + id + ")"; //厅店采购申请单
                entity["mcs_approvalstatus"] = bgy.spreadrule.utils.GetApprovalStatusText(data["mcs_rulestatus"]);//审批状态（草稿）
                CRM.Common.createSync("mcs_approvalrecordoveralls", entity, function (result) {
                    mcc.form.alert({ title: "消息提示", content: "执行【提交】成功", messagetype: mcc.form.messagetype.success });
                    Xrm.Page.data.refresh();//刷新页面                
                    bgy.spreadrule.utils.DisabledAllControl(true);//锁定所有的窗体不可以编辑
                }, function (req) {
                    mcc.form.alert({ title: "错误提示", content: JSON.parse(req.response).error.message, messagetype: mcc.form.messagetype.error });
                });

            }, function (req) {
                mcc.form.alert({ title: "错误提示", content: JSON.parse(req.response).error.message, messagetype: mcc.form.messagetype.error });
            });
        },
        AuditFactory: function (parameters) {
            if (!parameters) {
                mcc.form.alert({ title: "消息提示", content: "获取审批数据失败！", messagetype: mcc.form.messagetype.warning });
                mcc.form.closeLoading();
                return false;
            }

            var disabled = true;//页面元素禁用状态
            var data = new Object();
            var mcs_rulestatus = Xrm.Page.getAttribute("mcs_rulestatus").getValue();//审批状态
            if (parameters.auditStatus == 2) {
                //校验：是否等于驳回
                disabled = false;
                data["mcs_rulestatus"] =3;
            }
            else {
                //校验：审批通过
                //0:草稿;1:待审批;2:审批中;3:审批通过;4:驳回;
                data["mcs_rulestatus"] = 2;
            }

            //保存更新
            var entityId = Xrm.Page.data.getEntity().getId().replace("{", "").replace("}", "");//Id
            CRM.Common.updateSync("mcs_spreadrules", entityId, data, function (result) {
                //更新审批状态成功之后的操作：添加审批记录
                var entity = new Object();
                var userId = Xrm.Page.context.getUserId().replace("{", "").replace("}", "");
                entity["mcs_name"] = bgy.spreadrule.utils.GenOrderNo();//生成订单号
                entity["mcs_approvaldate"] = new Date();//系统当前时间
                entity["mcs_comments"] = parameters.contents;//审批意见
                entity["mcs_approvaluser@odata.bind"] = "/systemusers(" + userId + ")";//审批人
                entity["mcs_spreadruleid@odata.bind"] = "/mcs_spreadrules(" + entityId + ")"; //厅店采购申请单
                entity["mcs_approvalstatus"] = bgy.spreadrule.utils.GetApprovalStatusText(data["mcs_rulestatus"]);//审批状态
                CRM.Common.createSync("mcs_approvalrecordoveralls", entity, function (result) {
                    mcc.form.alert({ title: "消息提示", content: "执行【审批】成功", messagetype: mcc.form.messagetype.success });
                    Xrm.Page.data.refresh();//刷新页面
                    bgy.spreadrule.utils.DisabledAllControl(disabled);//锁定所有的窗体不可以编辑
                }, function (req) {
                    mcc.form.alert({ title: "错误提示", content: JSON.parse(req.response).error.message, messagetype: mcc.form.messagetype.error });
                });
            }, function (req) {
                mcc.form.alert({ title: "错误提示", content: JSON.parse(req.response).error.message, messagetype: mcc.form.messagetype.error });
            });
        },
        //锁定所有的窗体不可以编辑
        DisabledAllControl: function (disabled) {
            var optionsetControls = Xrm.Page.getControl(function (control, index) {
                var controlType = control.getControlType();
                return controlType != "iframe" && controlType != "webresource" && controlType != "subgrid";
            });
            optionsetControls.forEach(function (control, index) {
                //避免解锁表头字段，所有表头字段都是“header_”格式，故做此处理
                if (control.getName().indexOf("header_") == -1) {
                    control.setDisabled(disabled);
                }
            });
        },
        // 根据当前时间和随机数生成流水号
        GenOrderNo: function (len) {
            len = len || 32;
            var $chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
            var maxPos = $chars.length;
            var pwd = '';
            for (i = 0; i < len; i++) {
                //0~32的整数 
                pwd += $chars.charAt(Math.floor(Math.random() * (maxPos + 1)));
            }
            return pwd;
        },
        GetApprovalStatusText: function (approvalstatus) {
            var text = "草稿";
            //0:草稿;1:待审批;2:审批中;3:审批通过;4:驳回;
            switch (approvalstatus) {
                case 0:
                    text = "草稿";
                    break;
                case 1:
                    text = "已提交";
                    break;
                case 2:
                    text = "审批通过";
                    break;
                case 3:
                    text = "驳回";
                    break;
            }
            return text;
        },
    }
}