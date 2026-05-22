/**
 * CONFIDENTIAL AND PROPRIETARY SOURCE CODE.
 *
 * Use and distribution of this code is subject to applicable licenses and the
 * permission of the code owner. This notice does not indicate the actual or
 * intended publication of this source code.
 *
 * Portions developed for THAITUAN GROUP JOINT STOCK COMPANY by BTM Global Consulting LLC and are the
 * property of THAITUAN GROUP JOINT STOCK COMPANY.
 * =============================================================================
 * Version   Date            Author        Remarks
 * 1.0.0     2023/12/18      DuyK Tran     Initial Draft --- Tích hợp sản xuất.
 */

/**
 * @NApiVersion 2.1
 * @NScriptType MapReduceScript
 */
define(['N/search', 'N/record', '../lib/btm_tt_common', 'N/error'],

    (search, record, common, error) => {
        /**
         * Defines the function that is executed at the beginning of the map/reduce process and generates the input data.
         * @param {Object} inputContext
         * @param {boolean} inputContext.isRestarted - Indicates whether the current invocation of this function is the first
         *     invocation (if true, the current invocation is not the first invocation and this function has been restarted)
         * @param {Object} inputContext.ObjectRef - Object that references the input data
         * @typedef {Object} ObjectRef
         * @property {string|number} ObjectRef.id - Internal ID of the record instance that contains the input data
         * @property {string} ObjectRef.type - Type of the record instance that contains the input data
         * @returns {Array|Object|Search|ObjectRef|File|Query} The input data to use in the map/reduce process
         * @since 2015.2
         */
        const getInputData = (inputContext) => {
            try {
                const dataSearchObj = search.create({
                    type: 'customrecord_btm_tt_tp_sx',
                    filters:
                        [
                            ['custrecord_btm_tt_status_manu', 'anyof', '1']
                            // ['internalid', 'anyof', 66905, 66904] // For testing
                        ],
                    columns:
                        [
                            search.createColumn({
                                name: 'custrecord_btm_tt_m_dh_yc_sx',
                                label: 'Mã đơn hàng yêu cầu sản xuất'
                            }),
                            search.createColumn({ name: 'custrecord_btm_tt_mh_tp', label: 'Mã hàng thành phẩm' }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_bill_of_materials',
                                label: 'BILL OF MATERIALS'
                            }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_bill_of_materials_revi',
                                label: 'BILL OF MATERIALS REVISION'
                            }),
                            search.createColumn({ name: 'custrecord_btm_tt_thay_doi_bom', label: 'Có thay đổi BOM' }),
                            search.createColumn({ name: 'custrecord_btm_tt_ngay_sx', label: 'Ngày sản xuất' }),
                            search.createColumn({ name: 'custrecord_btm_tt_gio_bat_dau', label: 'Giờ bắt đầu' }),
                            search.createColumn({ name: 'custrecord_btm_tt_gio_ket_thuc', label: 'Giờ kết thúc' }),
                            search.createColumn({ name: 'custrecord_btm_tt_kt_cay_moc', label: 'Kết thúc cây mộc' }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_lot_serial_number',
                                label: 'Lot/Serial Number'
                            }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_sl_thanh_pham',
                                label: 'Số lượng thành phẩm'
                            }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_ma_chat_luong_tam',
                                label: 'Mã chất lượng'
                            }),
                            search.createColumn({ name: 'custrecord_btm_tt_sl_yard_tam', label: 'Số lượng Yard' }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_sl_cat_mau_tam',
                                label: 'Số lượng Cắt mẫu'
                            }),
                            search.createColumn({ name: 'custrecord_btm_tt_nguoi_kiem_tam', label: 'Người kiểm' }),
                            search.createColumn({ name: 'custrecord_btm_tt_raubien1_tam', label: 'Raubien1' }),
                            search.createColumn({ name: 'custrecord_btm_tt_raubien2_tam', label: 'Raubien2' }),
                            search.createColumn({ name: 'custrecord_btm_tt_so_bien_ban_tam', label: 'Số biên bản' }),
                            search.createColumn({ name: 'custrecord_btm_tt_loi_dau_tam', label: 'Lỗi dầu' }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_gia_thanh_kh_tam',
                                label: 'Giá thành kế hoạch'
                            }),
                            search.createColumn({ name: 'custrecord_btm_tt_ma_truc_tam', label: 'Mã trục' }),
                            search.createColumn({ name: 'custrecord_btm_tt_ma_loai_vai', label: 'Mã loại vải' }),
                            search.createColumn({ name: 'custrecord_btm_tt_datxk', label: 'DATXK' }),
                            search.createColumn({ name: 'custrecord_btm_tt_ngay_kiem', label: 'Ngày kiểm' }),
                            search.createColumn({ name: 'custrecord_btm_tt_wo_tam', label: 'Work Order' }),
                            search.createColumn({
                                name: 'custrecord_btm_tt_assembly_build_tam',
                                label: 'Assembly Build'
                            }),
                            search.createColumn({ name: 'custrecord_btm_tt_bpsx', label: 'Bộ phận sản xuất' }),
                            search.createColumn({ name: 'custrecord_btm_tt_mnsx', label: 'Nhà máy sản xuất' }),
                            search.createColumn({ name: 'custrecord_btm_tt_status_manu', label: 'Status' }),
                            search.createColumn({ name: 'custrecord_btm_tt_thay_doi_revision', label: 'Thay đổi Revision' }),
                            search.createColumn({ name: "internalid",  sort: search.Sort.ASC, label: "Internal ID"})
                        ]
                });
                return dataSearchObj;
            } catch (e) {
                log.error('Error btm_tt_mr_migrate_manufacturing > getInputData', e);
            }
        };

        /**
         * Lấy thông tin Thành Phẩm Sản Xuất
         * @param values
         * @return {{requireOrder: number,
         * assemblyItem: number,
         * assemblyItemText: string,
         * BOM: number,
         * bomRevision: number,
         * bomRevisionText: string,
         * isChangeBOM: 'T'|'F',
         * isChangeRevision: 'T'|'F',
         * manufactDate: string,
         * startTime: string,
         * endTime: string,
         * endTree: 'T'|'F',
         * humanCheck: number,
         * lotNumber: string,
         * munafactQty: string,
         * qualityCode: string,
         * raubien1: string,
         * raubien2: string,
         * yardQty: number,
         * cutQty: number,
         * reportNumber: number,
         * planAmount: number,
         * oilError: string,
         * axisCode: string,
         * fabricTypeCode: number,
         * datxk: string,
         * checkDate: string,
         * WO: number,
         * AB: number,
         * department: number,
         * location: number
         * }}
         */
        const getThanhPhamSanXuat = (values) => {
            /** @type {{
             * requireOrder: number,
             * assemblyItem: number,
             * assemblyItemText: string,
             * BOM: number,
             * bomRevision: number,
             * bomRevisionText: string,
             * isChangeBOM: 'T'|'F',
             * isChangeRevision: 'T'|'F',
             * manufactDate: string,
             * startTime: string,
             * endTime: string,
             * endTree: 'T'|'F',
             * humanCheck: number,
             * lotNumber: string,
             * munafactQty: string,
             * qualityCode: string,
             * raubien1: string,
             * raubien2: string,
             * yardQty: number,
             * cutQty: number,
             * reportNumber: number,
             * planAmount: number,
             * oilError: string,
             * axisCode: string,
             * fabricTypeCode: number,
             * datxk: string,
             * checkDate: string,
             * WO: number,
             * AB: number,
             * department: number,
             * location: number
             * }} */
            const searchObj = {};

            searchObj.requireOrder = values['custrecord_btm_tt_m_dh_yc_sx'].value;
            searchObj.assemblyItem = values['custrecord_btm_tt_mh_tp'].value;
            searchObj.assemblyItemText = values['custrecord_btm_tt_mh_tp'].text;
            searchObj.BOM = values['custrecord_btm_tt_bill_of_materials'].value;
            searchObj.bomRevision = values['custrecord_btm_tt_bill_of_materials_revi'].value;
            searchObj.bomRevisionText = values['custrecord_btm_tt_bill_of_materials_revi'].text;
            searchObj.isChangeBOM = values['custrecord_btm_tt_thay_doi_bom'];
            searchObj.isChangeRevision = values['custrecord_btm_tt_thay_doi_revision'];
            searchObj.manufactDate = values['custrecord_btm_tt_ngay_sx'];
            searchObj.startTime = values['custrecord_btm_tt_gio_bat_dau'];
            searchObj.endTime = values['custrecord_btm_tt_gio_ket_thuc'];
            searchObj.endTree = values['custrecord_btm_tt_kt_cay_moc'];
            searchObj.lotNumber = values['custrecord_btm_tt_lot_serial_number'];
            searchObj.munafactQty = values['custrecord_btm_tt_sl_thanh_pham'];
            searchObj.qualityCode = values['custrecord_btm_tt_ma_chat_luong_tam'];
            searchObj.yardQty = values['custrecord_btm_tt_sl_yard_tam'];
            searchObj.cutQty = values['custrecord_btm_tt_sl_cat_mau_tam'];
            searchObj.humanCheck = values['custrecord_btm_tt_nguoi_kiem_tam'].value;
            searchObj.raubien1 = values['custrecord_btm_tt_raubien1_tam'];
            searchObj.raubien2 = values['custrecord_btm_tt_raubien2_tam'];
            searchObj.reportNumber = values['custrecord_btm_tt_so_bien_ban_tam'];
            searchObj.oilError = values['custrecord_btm_tt_loi_dau_tam'];
            searchObj.planAmount = values['custrecord_btm_tt_gia_thanh_kh_tam'];
            searchObj.axisCode = values['custrecord_btm_tt_ma_truc_tam'];
            searchObj.fabricTypeCode = values['custrecord_btm_tt_ma_loai_vai'].value;
            searchObj.datxk = values['custrecord_btm_tt_datxk'];
            searchObj.checkDate = values['custrecord_btm_tt_ngay_kiem'];
            searchObj.WO = values['custrecord_btm_tt_wo_tam'].value;
            searchObj.AB = values['custrecord_btm_tt_assembly_build_tam'].value;
            searchObj.department = values['custrecord_btm_tt_bpsx'].value;
            searchObj.location = values['custrecord_btm_tt_mnsx'].value;
            log.debug('TPSX', searchObj);
            return searchObj;
        };

        /**
         * Lây thông tin Yêu cầu sản xuất
         * @param thanhPhamSanXuat
         * @return {null|{loai: {value: number, text: string}, thiTruong: {value: number, text: string}, soLuongYeuCau: number}}
         */
        const getDonYeuCauSanXuat = (thanhPhamSanXuat) => {
            /** @type {null|{loai: {value: number, text: string}, thiTruong: {value: number, text: string}, soLuongYeuCau: number}} */
            let donYC = null;

            const requireType = search.lookupFields({
                type: 'customrecord_btm_tt_don_yeu_cau_sx',
                id: thanhPhamSanXuat.requireOrder,
                columns: ['custrecord_btm_tt_don_yc_loai', 'custrecord_btm_tt_don_yc_loai', 'custrecord_btm_tt_don_yc_thi_truong']
            });
            switch (requireType['custrecord_btm_tt_don_yc_loai'][0].text) {
                case 'Mộc':
                    donYC = getYCMoc(thanhPhamSanXuat.requireOrder, thanhPhamSanXuat.assemblyItem, thanhPhamSanXuat.location);
                    break;
                case 'Vải thành phẩm':
                case 'Nhuộm':
                    donYC = getVaiThanhPham(thanhPhamSanXuat.requireOrder, thanhPhamSanXuat.assemblyItem, thanhPhamSanXuat.location);
                    break;
                case 'In':
                    donYC = getIn(thanhPhamSanXuat.requireOrder, thanhPhamSanXuat.assemblyItem, thanhPhamSanXuat.location);
                    break;
                case 'May':
                    donYC = getMay(thanhPhamSanXuat.requireOrder, thanhPhamSanXuat.assemblyItem, thanhPhamSanXuat.location);
                    break;
                case 'Đính đá':
                    donYC = getDinhDa(thanhPhamSanXuat.requireOrder, thanhPhamSanXuat.assemblyItem, thanhPhamSanXuat.location);
                    break;
                case 'Gia công sợi khác':
                case 'Gia Công sợi: Xe, chập, mắc,…':
                case 'Gia công sợi: Ghép, hấp,…':
                    donYC = getGiaCongSoi(thanhPhamSanXuat.requireOrder, thanhPhamSanXuat.assemblyItem, thanhPhamSanXuat.location);
                    break;
                default:
                    break;
            }
            if (donYC) {
                donYC.loai = requireType['custrecord_btm_tt_don_yc_loai'][0];
                donYC.thiTruong = requireType['custrecord_btm_tt_don_yc_thi_truong'][0];
            }
            log.debug('YCSX', donYC);
            return donYC;
        };

        /**
         * Defines the function that is executed when the map entry point is triggered. This entry point is triggered automatically
         * when the associated getInputData stage is complete. This function is applied to each key-value pair in the provided
         * context.
         * @param {Object} mapContext - Data collection containing the key-value pairs to process in the map stage. This parameter
         *     is provided automatically based on the results of the getInputData stage.
         * @param {Iterator} mapContext.errors - Serialized errors that were thrown during previous attempts to execute the map
         *     function on the current key-value pair
         * @param {number} mapContext.executionNo - Number of times the map function has been executed on the current key-value
         *     pair
         * @param {boolean} mapContext.isRestarted - Indicates whether the current invocation of this function is the first
         *     invocation (if true, the current invocation is not the first invocation and this function has been restarted)
         * @param {string} mapContext.key - Key to be processed during the map stage
         * @param {string} mapContext.value - Value to be processed during the map stage
         * @since 2015.2
         */

        const map = (mapContext) => {
            try {
                const INTEGRATE_RECORD = 'customrecord_btm_tt_tp_sx';
                // log.debug('mapContext',mapContext);
                let values = JSON.parse(mapContext.value).values;

                //region Lấy thông tin record TPSX - thanhPhamSanXuat
                const thanhPhamSanXuat = getThanhPhamSanXuat(values);
                thanhPhamSanXuat.id = mapContext.key;
                //endregion Lấy thông tin record TPSX

                //region Lấy thông tin Đơn YC SX - yeuCauSanXuat
                const yeuCauSanXuat = getDonYeuCauSanXuat(thanhPhamSanXuat);
                //endregion Lấy thông tin Đơn YC SX

                let woInfo = getWO(thanhPhamSanXuat.requireOrder, thanhPhamSanXuat.assemblyItem, thanhPhamSanXuat.location);

                let woId;

                if (thanhPhamSanXuat.isChangeBOM === 'T' || thanhPhamSanXuat.isChangeRevision === 'T' || !woInfo.id) {

                    //region Kiem tra va tao mat hang ban thanh pham - childItemId
                    const cfgRecord = common.getConfigRecord({ recordType: 'customrecord_btm_tt_configuration' });
                    let childItemId;
                    const findChildItem = checkChildItem(`BTP(154)_${thanhPhamSanXuat.assemblyItemText}`);
                    if (findChildItem) {
                        childItemId = findChildItem;
                    } else {
                        //Create child item
                        const childItem = record.create({
                            type: record.Type.ASSEMBLY_ITEM
                        });

                        const taxCodeItem = search.lookupFields({
                            type: record.Type.ASSEMBLY_ITEM,
                            id: thanhPhamSanXuat.assemblyItem,
                            columns: ['salestaxcode']
                        }).salestaxcode[0].value;

                        childItem.setValue('itemid', `BTP(154)_${thanhPhamSanXuat.assemblyItemText}`);
                        childItem.setValue('assetaccount', cfgRecord.getValue('custrecord_btm_tt_cfg_acc_1541'));
                        childItem.setValue('custitem_btm_mc_for_prod', true);
                        childItem.setValue('salestaxcode', taxCodeItem);
                        childItemId = childItem.save();
                    }
                    //endregion Kiem tra va tao mat hang ban thanh pham

                    let nameBOM = null;

                    //region Xử lý bỏ default master ở BOM cũ nếu TPSX được đánh dấu là Change BOM
                    if (thanhPhamSanXuat.isChangeBOM === 'T') {
                        handleOldBOM(childItemId);
                        handleOldBOM(thanhPhamSanXuat.assemblyItem);
                    }
                    //endregion Xử lý bỏ default master ở BOM cũ

                    //region Thay the mat hang thanh pham bang mat hang ban thanh pham tren BOM chinh
                    const parentBOMRcrd = record.load({
                        type: record.Type.BOM,
                        id: thanhPhamSanXuat.BOM,
                        isDynamic: true
                    });
                    nameBOM = parentBOMRcrd.getValue('name');
                    const assemblyLine = parentBOMRcrd.findSublistLineWithValue({
                        sublistId: 'assembly',
                        fieldId: 'assembly',
                        value: thanhPhamSanXuat.assemblyItem
                    });

                    const childAssemblyLine = parentBOMRcrd.findSublistLineWithValue({
                        sublistId: 'assembly',
                        fieldId: 'assembly',
                        value: childItemId
                    });

                    if (assemblyLine > -1) {
                        parentBOMRcrd.removeLine({
                            sublistId: 'assembly',
                            line: assemblyLine,
                            ignoreRecalc: true
                        });
                    }

                    //Add child item into current BOM
                    if (childAssemblyLine < 0) {
                        let existingValues = parentBOMRcrd.getValue({
                            fieldId: 'restricttoassemblies'
                        });
                        if (Array.isArray(existingValues)) {
                            parentBOMRcrd.setValue({
                                fieldId: 'restricttoassemblies',
                                value: [childItemId, ...existingValues]
                            });
                        } else {
                            parentBOMRcrd.setValue({
                                fieldId: 'restricttoassemblies',
                                value: [childItemId, existingValues]
                            });
                        }
                        // log.debug('Restriction', parentBOMRcrd.getValue({
                        //     fieldId: 'restricttoassemblies'
                        // }));
                        parentBOMRcrd.selectNewLine('assembly');
                        parentBOMRcrd.setCurrentSublistValue({
                            sublistId: 'assembly',
                            fieldId: 'assembly',
                            value: childItemId
                        });
                        parentBOMRcrd.setCurrentSublistValue({
                            sublistId: 'assembly',
                            fieldId: 'masterdefault',
                            value: true
                        });
                        parentBOMRcrd.commitLine('assembly');
                    }else{
                        log.debug('Update Parent BOM',{parentBOMRcrd: parentBOMRcrd.id ,childAssemblyLine});

                        parentBOMRcrd.selectLine({
                            sublistId: 'assembly',
                            line: childAssemblyLine
                        });

                        let isMaster = parentBOMRcrd.getCurrentSublistValue({
                            sublistId: 'assembly',
                            fieldId: 'masterdefault',
                        });
                        parentBOMRcrd.commitLine('assembly');

                        if(!isMaster){
                            parentBOMRcrd.removeLine({
                                sublistId: 'assembly',
                                line: childAssemblyLine,
                                ignoreRecalc: true
                            });
                            parentBOMRcrd.selectNewLine('assembly');
                            parentBOMRcrd.setCurrentSublistValue({
                                sublistId: 'assembly',
                                fieldId: 'assembly',
                                value: childItemId
                            });
                            parentBOMRcrd.setCurrentSublistValue({
                                sublistId: 'assembly',
                                fieldId: 'masterdefault',
                                value: true
                            });
                            parentBOMRcrd.commitLine('assembly');
                        }
                    }
                    parentBOMRcrd.save();
                    //endregion Thay the mat hang thanh pham bang mat hang ban thanh pham tren BOM chinh

                    //region Kiem tra va Tao BOM ban thanh pham cho BOM chinh - childBOMRcrdId
                    let childBOMRcrdId = checkChildBOM(`BTP(154)_${nameBOM}`);
                    if (!childBOMRcrdId) {
                        const childBOMRcrd = record.create({
                            type: record.Type.BOM
                        });
                        childBOMRcrd.setValue('name', `BTP(154)_${nameBOM}`);
                        childBOMRcrdId = childBOMRcrd.save();
                    }
                    //Add Assemblies
                    const childBOMUpdateRcrd = record.load({
                        type: record.Type.BOM,
                        id: childBOMRcrdId,
                        isDynamic: true
                    });

                    let existingAssemblyIndex = childBOMUpdateRcrd.findSublistLineWithValue({
                        sublistId: 'assembly',
                        fieldId: 'assembly',
                        value: thanhPhamSanXuat.assemblyItem
                    });

                    if (existingAssemblyIndex < 0) {

                        let existingValues = childBOMUpdateRcrd.getValue({
                            fieldId: 'restricttoassemblies'
                        });
                        if (Array.isArray(existingValues)) {
                            childBOMUpdateRcrd.setValue({
                                fieldId: 'restricttoassemblies',
                                value: [thanhPhamSanXuat.assemblyItem, ...existingValues]
                            });
                        } else {
                            childBOMUpdateRcrd.setValue({
                                fieldId: 'restricttoassemblies',
                                value: [thanhPhamSanXuat.assemblyItem, existingValues]
                            });
                        }
                        childBOMUpdateRcrd.selectNewLine('assembly');
                        childBOMUpdateRcrd.setCurrentSublistValue({
                            sublistId: 'assembly',
                            fieldId: 'assembly',
                            value: thanhPhamSanXuat.assemblyItem
                        });
                        childBOMUpdateRcrd.setCurrentSublistValue({
                            sublistId: 'assembly',
                            fieldId: 'masterdefault',
                            value: true
                        });
                        childBOMUpdateRcrd.commitLine('assembly');
                    }else{
                        log.debug('Update child BOM',childBOMUpdateRcrd.id);

                        childBOMUpdateRcrd.selectLine({
                            sublistId: 'assembly',
                            line: existingAssemblyIndex
                        });

                        let isMaster = parentBOMRcrd.getCurrentSublistValue({
                            sublistId: 'assembly',
                            fieldId: 'masterdefault',
                        });
                        childBOMUpdateRcrd.commitLine('assembly');

                        if(!isMaster) {

                            childBOMUpdateRcrd.removeLine({
                                sublistId: 'assembly',
                                line: existingAssemblyIndex,
                                ignoreRecalc: true
                            });
                            childBOMUpdateRcrd.selectNewLine('assembly');
                            childBOMUpdateRcrd.setCurrentSublistValue({
                                sublistId: 'assembly',
                                fieldId: 'assembly',
                                value: thanhPhamSanXuat.assemblyItem
                            });
                            childBOMUpdateRcrd.setCurrentSublistValue({
                                sublistId: 'assembly',
                                fieldId: 'masterdefault',
                                value: true
                            });
                            childBOMUpdateRcrd.commitLine('assembly');
                        }
                    }
                    childBOMUpdateRcrd.save();

                    //Create child revision
                    let childBOMRev = checkChildBOMRev(`BTP(154)_${thanhPhamSanXuat.bomRevisionText}`);
                    const bomRevInfo = getBOMRev(thanhPhamSanXuat.bomRevisionText);
                    if (!childBOMRev) {
                        const childRevisonRcrd = record.create({
                            type: record.Type.BOM_REVISION,
                            isDynamic: true
                        });
                        childRevisonRcrd.setValue('billofmaterials', childBOMRcrdId);
                        childRevisonRcrd.setValue('name', `BTP(154)_${thanhPhamSanXuat.bomRevisionText}`);
                        childRevisonRcrd.setText('effectivestartdate', bomRevInfo.startDate);
                        childRevisonRcrd.setText('effectiveenddate', bomRevInfo.endDate);

                        childRevisonRcrd.selectNewLine('component');

                        childRevisonRcrd.setCurrentSublistValue({
                            sublistId: 'component',
                            fieldId: 'item',
                            value: childItemId
                        });

                        childRevisonRcrd.setCurrentSublistValue({
                            sublistId: 'component',
                            fieldId: 'itemsource',
                            value: 'WORK_ORDER' //Work Order
                        });

                        childRevisonRcrd.commitLine('component');
                        childRevisonRcrd.save();
                    }
                    //endregion Kiem tra va Tao BOM ban thanh pham cho BOM chinh

                    let woRcrd = record.create({
                        type: record.Type.WORK_ORDER,
                        isDynamic: true
                    });
                    woRcrd.setValue('assemblyitem', thanhPhamSanXuat.assemblyItem);
                    // woRcrd.setValue('billofmaterials', childBOMRcrdId);
                    // woRcrd.setValue('billofmaterialsrevision', childRevisonRcrdId);
                    woRcrd.setValue('quantity', yeuCauSanXuat.soLuongYeuCau);
                    woRcrd.setValue('department', thanhPhamSanXuat.department);
                    woRcrd.setValue('location', thanhPhamSanXuat.location);
                    woRcrd.setValue('custbody_btm_tt_ma_don_yeu_cau', thanhPhamSanXuat.requireOrder);
                    woRcrd.setValue('custbody_btm_tt_loai_yeu_cau_dat_hang', yeuCauSanXuat.loai.value);
                    woRcrd.setValue('cseg_btm_tt_pv_bh', yeuCauSanXuat.thiTruong.text === 'ND' ? 2 : 1);//2: Nội địa, 1: Quốc tế
                    woRcrd.setText('trandate', thanhPhamSanXuat.manufactDate);
                    //Nếu data đúng thì WO sẽ tự động add line, vì vậy k cần set line cho WO.

                    woId = woRcrd.save();

                    woRcrd = record.load({
                        type: record.Type.WORK_ORDER,
                        id: woId
                    });

                    const lineCount = woRcrd.getLineCount('item');

                    for (let idx = 0; idx < lineCount; idx++) {

                        // const item = woRcrd.getSublistValue({
                        //     sublistId: 'item',
                        //     fieldId: 'item',
                        //     line: idx
                        // });
                        // const quantity = woRcrd.getSublistValue({
                        //     sublistId: 'item',
                        //     fieldId: 'quantity',
                        //     line: idx
                        // });

                        // log.debug(`Item: ${idx}`, {item,quantity});

                        woRcrd.setSublistValue({
                            sublistId: 'item',
                            fieldId: 'quantity',
                            line: idx,
                            value: yeuCauSanXuat.soLuongYeuCau
                        });
                        woRcrd.setSublistValue({
                            sublistId: 'item',
                            fieldId: 'bomquantity',
                            line: idx,
                            value: ''
                        });
                    }

                    woId = woRcrd.save();

                    log.debug('Create WO', {
                        WO: woId,
                        manufacturingItem: thanhPhamSanXuat.id,
                        requireOrder: thanhPhamSanXuat.requireOrder,
                        assemblyitem: thanhPhamSanXuat.assemblyItem
                    });

                } else if (woInfo.id && thanhPhamSanXuat.isChangeBOM === 'F') {

                    woId = woInfo.id;

                    log.debug('Update WO', {
                        WO: woId,
                        manufacturingItem: thanhPhamSanXuat.id,
                        requireOrder: thanhPhamSanXuat.requireOrder,
                        assemblyitem: thanhPhamSanXuat.assemblyItem
                    });
                }
                //End create Work Order

                const childWOId = findChildWO(woId);

                record.submitFields({
                    type: 'customrecord_btm_tt_tp_sx',
                    id: mapContext.key,
                    values: {
                        'custrecord_btm_tt_wo_tam': woId,
                        'custrecord_btm_tt_wo_btp': childWOId || ''
                    }
                });

                //Sao phải cực khổ dữ vậy?
                mapContext.write({
                    key: `${thanhPhamSanXuat.requireOrder}-${thanhPhamSanXuat.assemblyItem}-${thanhPhamSanXuat.BOM}-${thanhPhamSanXuat.bomRevision}-${thanhPhamSanXuat.manufactDate}-${woId}`,
                    value: `${JSON.stringify(thanhPhamSanXuat)}&&${JSON.stringify(yeuCauSanXuat)}`
                });

            } catch (e) {
                log.error('Error.Map', e);
                record.submitFields({
                    type: 'customrecord_btm_tt_tp_sx',
                    id: mapContext.key,
                    values: {
                        'custrecord_btm_tt_status_manu': 4,//Lỗi
                        'custrecord_btm_tt_ghi_chu': `${e.message}\n${e?.stack && e?.stack[0]}`
                    }
                });
            }
        };

        /**
         * Defines the function that is executed when the reduce entry point is triggered. This entry point is triggered
         * automatically when the associated map stage is complete. This function is applied to each group in the provided context.
         * @param {Object} reduceContext - Data collection containing the groups to process in the reduce stage. This parameter is
         *     provided automatically based on the results of the map stage.
         * @param {Iterator} reduceContext.errors - Serialized errors that were thrown during previous attempts to execute the
         *     reduce function on the current group
         * @param {number} reduceContext.executionNo - Number of times the reduce function has been executed on the current group
         * @param {boolean} reduceContext.isRestarted - Indicates whether the current invocation of this function is the first
         *     invocation (if true, the current invocation is not the first invocation and this function has been restarted)
         * @param {string} reduceContext.key - Key to be processed during the reduce stage
         * @param {List<String>} reduceContext.values - All values associated with a unique key that was passed to the reduce stage
         *     for processing
         * @since 2015.2
         */

        const reduce = (reduceContext) => {

            try {
                const { values, key } = reduceContext;
                const woId = key.split('-')[5];

                log.debug('values', values);

                let searchObj = values[0].split('&&')[0];
                searchObj = JSON.parse(searchObj);

                let requireObj = values[0].split('&&')[1];
                requireObj = JSON.parse(requireObj);

                let munafactQtyTotal = 0;

                values.forEach((element) => {
                    element = JSON.parse(element.split('&&')[0]);
                    munafactQtyTotal += +element.munafactQty;
                });
                munafactQtyTotal = munafactQtyTotal.toFixed(2);
                //get require manufacturing type

                const childWOId = findChildWO(woId);
                let childABId;
                // Update quantity for Child Work Order
                // let tranDate = searchObj.manufactDate.split('/');

                const childWORcrd = record.load({
                    type: record.Type.WORK_ORDER,
                    id: childWOId,
                    isDynamic: true
                });

                childWORcrd.setValue('quantity', requireObj.soLuongYeuCau);
                childWORcrd.setText('trandate', searchObj.manufactDate);
                childWORcrd.save();

                //Xử lý giá thành
                const { data: assemblyCostArr, totalRemaining } = findAssemblyCost(childWOId);
                log.debug('Assembly Cost Information', { assemblyCostArr, totalRemaining });
                let manufactQtyNeed = munafactQtyTotal;
                let isNeedAssembly = true;

                if (totalRemaining > 0) {

                    if (manufactQtyNeed > totalRemaining) {

                        manufactQtyNeed = manufactQtyNeed - totalRemaining;

                        assemblyCostArr.forEach((element) => {
                            record.submitFields({
                                type: record.Type.ASSEMBLY_BUILD,
                                id: element.id,
                                values: {
                                    custbody_btm_mc_ass_rem_qty: 0
                                }
                            });
                        });

                    } else {

                        isNeedAssembly = false;

                        for (let idx = 0; idx < assemblyCostArr.length; idx++) {

                            let element = assemblyCostArr[idx];

                            let assemblyCostRcrd = record.load({
                                type: record.Type.ASSEMBLY_BUILD,
                                id: element.id
                            });

                            let remainingQty = +assemblyCostRcrd.getValue('custbody_btm_mc_ass_rem_qty');

                            if (manufactQtyNeed > remainingQty) {
                                manufactQtyNeed -= remainingQty;
                                remainingQty = 0;
                            } else {
                                remainingQty = remainingQty - manufactQtyNeed;
                                manufactQtyNeed = 0;
                            }
                            assemblyCostRcrd.setValue('custbody_btm_mc_ass_rem_qty', remainingQty);
                            assemblyCostRcrd.save();

                            if (manufactQtyNeed === 0) {
                                break;
                            }
                        }
                    }
                }
                // Create the Child Assembly Build record
                if (isNeedAssembly) {
                    let assemblyBuildRec = record.transform({
                        fromType: record.Type.WORK_ORDER,
                        fromId: childWOId,
                        toType: record.Type.ASSEMBLY_BUILD,
                        isDynamic: true
                    });
                    // log.debug('childWOId', childWOId);

                    assemblyBuildRec.setValue({ fieldId: 'quantity', value: manufactQtyNeed });
                    assemblyBuildRec.setValue({
                        fieldId: 'custbody_btm_tt_kt_cay_moc',
                        value: searchObj.endTree === 'F' ? false : true
                    });
                    // assemblyBuildRec.setValue({ fieldId: 'custbody_btm_tt_nguoi_nhap', value: searchObj.humanCheck});
                    assemblyBuildRec.setText({ fieldId: 'custbody_btm_tt_gio_bat_dau', value: searchObj.startTime });
                    assemblyBuildRec.setText({ fieldId: 'custbody_btm_tt_gio_ket_thuc', value: searchObj.endTime });
                    assemblyBuildRec.setText('trandate', searchObj.manufactDate);

                    //region 1.0 - Create Inventory Detail records for lines
                    const workOrderRec = record.load({
                        type: record.Type.WORK_ORDER,
                        id: woId
                    });
                    // log.debug('woId', woId);

                    const maDonHangYeuCauSX = workOrderRec.getValue({ fieldId: 'custbody_btm_tt_ma_don_yeu_cau' });
                    const location = workOrderRec.getValue({ fieldId: 'location' });
                    let inventoryItems = [];

                    //Kiểm tra âm kho, nếu âm kho thì trả lỗi
                    for (let i = 0; i < assemblyBuildRec.getLineCount({ sublistId: 'component' }); i++) {
                        assemblyBuildRec.selectLine('component', i);
                        const item = assemblyBuildRec.getCurrentSublistValue({
                            sublistId: 'component',
                            fieldId: 'item'
                        });
                        const itemLookup = search.lookupFields({
                            type: search.Type.ITEM,
                            id: item,
                            columns: ['islotitem', 'type']
                        });
                        if (itemLookup.type[0].value === 'InvtPart' || itemLookup.type[0].value === 'Assembly') {
                            inventoryItems.push(item);
                        }
                    }

                    const itemNegatives = checkNegativeInventory(inventoryItems, searchObj.location);
                    if (itemNegatives.length > 0) {
                        let e = error.create({
                            name: 'INVENTORY_ERROR',
                            message: `Tồn kho : ${itemNegatives.join(', ')} không được phép âm`,
                            notifyOff: false
                        });
                        throw e;
                    }

                    for (let i = 0; i < assemblyBuildRec.getLineCount({ sublistId: 'component' }); i++) {
                        assemblyBuildRec.selectLine('component', i);
                        const item = assemblyBuildRec.getCurrentSublistValue({
                            sublistId: 'component',
                            fieldId: 'item'
                        });
                        let detailQty = assemblyBuildRec.getCurrentSublistValue({
                            sublistId: 'component',
                            fieldId: 'quantity'
                        });

                        let qty = detailQty;
                        let qtyOrgin = detailQty;

                        const itemLookup = search.lookupFields({
                            type: search.Type.ITEM,
                            id: item,
                            columns: 'islotitem'
                        });
                        // log.debug('itemLookup',itemLookup);
                        if (itemLookup.islotitem) {
                            const inventoryDetailLineRec = assemblyBuildRec.getCurrentSublistSubrecord({
                                sublistId: 'component',
                                fieldId: 'componentinventorydetail'
                            });
                            // log.debug('Item component', {item,detailQty: inventoryDetailLineRec.getValue('quantity')});

                            const lotNumberList = [];

                            const filters = [
                                ['item', 'anyof', item],
                                'AND',
                                ['location', 'anyof', location]
                            ];

                            if (maDonHangYeuCauSX) {
                                const wareHouseList = getWareHouse(maDonHangYeuCauSX);
                                if (wareHouseList.length > 0) {
                                    filters.push('AND');
                                    filters.push(['custitemnumber_btm_tt_ma_don_ycsx', 'anyof', maDonHangYeuCauSX]);
                                }
                            }

                            // log.debug('filters',filters);
                            let inventorynumerSearchObj = search.create({
                                type: 'inventorynumber',
                                filters: filters,
                                columns:
                                    [
                                        search.createColumn({ name: 'quantityavailable', label: 'Available' }),
                                        search.createColumn({
                                            name: 'inventorynumber',
                                            sort: search.Sort.ASC,
                                            label: 'Number'
                                        })
                                    ]
                            });
                            inventorynumerSearchObj.run().each(function(result) {

                                const id = result.id;

                                const quantity = +result.getValue({
                                    name: 'quantityavailable', label: 'Available'
                                });

                                const number = result.getValue({
                                    name: 'inventorynumber',
                                    sort: search.Sort.ASC,
                                    label: 'Number'
                                });

                                if (quantity > 0) {
                                    lotNumberList.push({
                                        id,
                                        number,
                                        quantity
                                    });
                                }
                                return true;

                            });

                            let qtyTotal = 0;
                            // log.debug('lotNumberList',lotNumberList);
                            for (let jdx = 0; jdx < lotNumberList.length; jdx++) {

                                let validQty;
                                let remainQty;
                                if (qty <= lotNumberList[jdx].quantity) {
                                    validQty = +qty;
                                } else {
                                    validQty = lotNumberList[jdx].quantity;
                                    qty -= validQty;
                                }

                                qtyTotal += +validQty;

                                log.debug('',{qtyTotal,qtyOrgin});

                                if (qtyTotal <= qtyOrgin) {
                                    inventoryDetailLineRec.selectNewLine('inventoryassignment');
                                    inventoryDetailLineRec.setCurrentSublistValue({
                                        sublistId: 'inventoryassignment',
                                        fieldId: 'issueinventorynumber',
                                        value: lotNumberList[jdx].id
                                    });
                                    inventoryDetailLineRec.setCurrentSublistValue({
                                        sublistId: 'inventoryassignment',
                                        fieldId: 'quantity',
                                        value: validQty
                                    });
                                    inventoryDetailLineRec.commitLine('inventoryassignment');
                                } else {
                                    break;
                                }
                            }
                            assemblyBuildRec.commitLine('component');
                        }
                    }
                    //endregion

                    // Save the child assembly record
                    childABId = assemblyBuildRec.save({
                        enableSourcing: true,
                        ignoreMandatoryFields: true
                    });
                }
                if (childABId) {
                    for (let idx = 0; idx < values.length; idx++) {
                        const value = JSON.parse(values[idx].split('&&')[0]);
                        record.submitFields({
                            type: 'customrecord_btm_tt_tp_sx',
                            id: value.id,
                            values: {
                                'custrecord_btm_tt_status_manu': 2,//Đang sản xuất
                                'custrecord_btm_tt_ab_btp': childABId
                            }
                        });
                    }
                }

                // Create the Parent Inventory Detail record
                let parentAssemblyBuildRec = record.transform({
                    fromType: record.Type.WORK_ORDER,
                    fromId: woId,
                    toType: record.Type.ASSEMBLY_BUILD,
                    isDynamic: true
                });

                parentAssemblyBuildRec.setValue({ fieldId: 'quantity', value: munafactQtyTotal });
                parentAssemblyBuildRec.setValue({
                    fieldId: 'custbody_btm_tt_kt_cay_moc',
                    value: searchObj.endTree === 'F' ? false : true
                });
                // parentAssemblyBuildRec.setValue({ fieldId: 'custbody_btm_tt_nguoi_nhap', value: searchObj.humanCheck});
                parentAssemblyBuildRec.setText({ fieldId: 'custbody_btm_tt_gio_bat_dau', value: searchObj.startTime });
                parentAssemblyBuildRec.setText({ fieldId: 'custbody_btm_tt_gio_ket_thuc', value: searchObj.endTime });
                parentAssemblyBuildRec.setText('trandate', searchObj.manufactDate);

                //Kiểm tra âm kho, nếu âm kho thì trả lỗi
                let inventoryItems = [];
                for (let i = 0; i < parentAssemblyBuildRec.getLineCount({ sublistId: 'component' }); i++) {
                    parentAssemblyBuildRec.selectLine('component', i);
                    const item = parentAssemblyBuildRec.getCurrentSublistValue({
                        sublistId: 'component',
                        fieldId: 'item'
                    });
                    inventoryItems.push(item);
                }

                const itemNegatives = checkNegativeInventory(inventoryItems, searchObj.location);
                if (itemNegatives.length > 0) {
                    let e = error.create({
                        name: 'INVENTORY_ERROR',
                        message: `Tồn kho: ${itemNegatives.join(', ')} không được phép âm`,
                        notifyOff: false
                    });
                    throw e;
                }

                let inventoryDetailRec = parentAssemblyBuildRec.getSubrecord({ fieldId: 'inventorydetail' });

                for (let idx = 0; idx < values.length; idx++) {
                    const value = JSON.parse(values[idx].split('&&')[0]);
                    inventoryDetailRec.selectLine('inventoryassignment', idx);
                    inventoryDetailRec.setCurrentSublistValue({
                        sublistId: 'inventoryassignment',
                        fieldId: 'receiptinventorynumber',
                        value: value.lotNumber
                    });
                    inventoryDetailRec.setCurrentSublistValue({
                        sublistId: 'inventoryassignment',
                        fieldId: 'quantity',
                        value: value.munafactQty
                    });
                    inventoryDetailRec.commitLine('inventoryassignment');
                }

                const assemblyBuildRecId = parentAssemblyBuildRec.save({
                    enableSourcing: true,
                    ignoreMandatoryFields: true
                });

                // Set details to the Inventory Detail record

                parentAssemblyBuildRec = record.load({
                    type: record.Type.ASSEMBLY_BUILD,
                    id: assemblyBuildRecId
                });

                inventoryDetailRec = parentAssemblyBuildRec.getSubrecord({ fieldId: 'inventorydetail' });

                for (let idx = 0; idx < values.length; idx++) {
                    const value = JSON.parse(values[idx].split('&&')[0]);
                    const inventoryNumberRecId = inventoryDetailRec.getSublistValue({
                        sublistId: 'inventoryassignment',
                        fieldId: 'numberedrecordid',
                        line: idx
                    });

                    record.submitFields({
                        type: record.Type.INVENTORY_NUMBER,
                        id: inventoryNumberRecId,
                        values: {
                            'custitemnumber_btm_tt_ma_qc': value.qualityCode,
                            'custitemnumber_btm_tt_sl_yard': value.yardQty,
                            'custitemnumber_btm_tt_sl_cat_mau': value.cutQty,
                            'custitemnumber_btm_tt_nguoi_kiem': value.humanCheck,
                            'custitemnumber_btm_tt_raubien1': value.raubien1,
                            'custitemnumber_btm_tt_raubien2': value.raubien2,
                            'custitemnumber_btm_tt_so_bien_ban': value.reportNumber,
                            'custitemnumber_btm_tt_loi_dau': value.oilError,
                            'custitemnumber_btm_tt_gia_thanh_ke_hoach': value.planAmount,
                            'custitemnumber_btm_tt_ma_truc': value.axisCode,
                            // 'custitemnumber_btm_tt_ma_loai_vai': value.fabricTypeCode,
                            'custitemnumber_btm_tt_datxk': value.datxk,
                            'custitemnumber_btm_tt_ngay_kiem': value.checkDate,
                            'custitemnumber_btm_ngay_tinh_tuoi_kho': value.manufactDate,
                            'custitemnumber_btm_tt_ma_don_ycsx': value.requireOrder,
                            'custitemnumber_btm_tt_tp_sx': value.id
                        }
                    });
                }

                if (assemblyBuildRecId) {
                    for (let idx = 0; idx < values.length; idx++) {
                        const value = JSON.parse(values[idx].split('&&')[0]);
                        record.submitFields({
                            type: 'customrecord_btm_tt_tp_sx',
                            id: value.id,
                            values: {
                                'custrecord_btm_tt_assembly_build_tam': assemblyBuildRecId,
                            }
                        });
                    }
                }
                reduceContext.write({
                    key: `${searchObj.id}`,
                    value: `${JSON.stringify(searchObj)}&&${JSON.stringify(requireObj)}`
                });

            } catch (e) {
                log.error('Error.Reduce', e);

                const { values, key } = reduceContext;

                values.forEach((element) => {
                    element = JSON.parse(element.split('&&')[0]);
                    record.submitFields({
                        type: 'customrecord_btm_tt_tp_sx',
                        id: element.id,
                        values: {
                            'custrecord_btm_tt_status_manu': 4,//Lỗi
                            'custrecord_btm_tt_ghi_chu': `${e.message}\n${e?.stack && e?.stack[0]}`
                        }
                    });
                });
            }
        };

        /**
         * Defines the function that is executed when the summarize entry point is triggered. This entry point is triggered
         * automatically when the associated reduce stage is complete. This function is applied to the entire result set.
         * @param {Object} summaryContext - Statistics about the execution of a map/reduce script
         * @param {number} summaryContext.concurrency - Maximum concurrency number when executing parallel tasks for the map/reduce
         *     script
         * @param {Date} summaryContext.dateCreated - The date and time when the map/reduce script began running
         * @param {boolean} summaryContext.isRestarted - Indicates whether the current invocation of this function is the first
         *     invocation (if true, the current invocation is not the first invocation and this function has been restarted)
         * @param {Iterator} summaryContext.output - Serialized keys and values that were saved as output during the reduce stage
         * @param {number} summaryContext.seconds - Total seconds elapsed when running the map/reduce script
         * @param {number} summaryContext.usage - Total number of governance usage units consumed when running the map/reduce
         *     script
         * @param {number} summaryContext.yields - Total number of yields when running the map/reduce script
         * @param {Object} summaryContext.inputSummary - Statistics about the input stage
         * @param {Object} summaryContext.mapSummary - Statistics about the map stage
         * @param {Object} summaryContext.reduceSummary - Statistics about the reduce stage
         * @since 2015.2
         */
        const summarize = (summaryContext) => {

            summaryContext.output.iterator().each(function(key, value) {

                let searchObj = value.split('&&')[0];
                searchObj = JSON.parse(searchObj);

                let requireObj = value.split('&&')[1];
                requireObj = JSON.parse(requireObj);

                //Region Close Work Order

                const {
                    sum,
                    woList
                } = getFinishManuItem(searchObj.requireOrder, searchObj.assemblyItem, searchObj.location);

                if (sum === requireObj.soLuongYeuCau) {

                    log.debug('Close Work Order', { sum, woList });

                    woList.forEach((wo) => {

                        let woRcrd = record.load({
                            type: record.Type.WORK_ORDER,
                            id: wo
                        });

                        woRcrd.setValue({
                            fieldId: 'orderstatus',
                            value: 'H'
                        });

                        woRcrd.setValue({
                            fieldId: 'origstatus',
                            value: 'H'
                        });

                        const lineCount = woRcrd.getLineCount('item');

                        for (let idx = 0; idx < lineCount; idx++) {
                            woRcrd.setSublistValue({
                                sublistId: 'item',
                                fieldId: 'isclosed',
                                line: idx,
                                value: true
                            });
                        }

                        woRcrd.save();
                    });

                    const fishManu = getFishishManuWO(woList);

                    fishManu.forEach((ele) => {
                        record.submitFields({
                            type: 'customrecord_btm_tt_tp_sx',
                            id: ele,
                            values: {
                                'custrecord_btm_tt_status_manu': 3//Hoàn thành
                            }
                        });
                    });
                    //end region
                }

                return true;
            });
        };

        const getFishishManuWO = (woList) => {
            let data = [];
            let tp_sxSearchObj = search.create({
                type: 'customrecord_btm_tt_tp_sx',
                filters:
                    [
                        ['custrecord_btm_tt_wo_tam', 'anyof', woList]
                    ],
                columns:
                    [
                        search.createColumn({
                            name: 'custrecord_btm_tt_m_dh_yc_sx',
                            label: 'Mã đơn hàng yêu cầu sản xuất'
                        })
                    ]
            });
            tp_sxSearchObj.run().each(function(result) {
                data.push(result.id);
                return true;
            });
            return data;
        };

        const getFinishManuItem = (requireOrder, assembly, location) => {
            let sum = 0;
            let woList = [];
            let tp_sxSearchObj = search.create({
                type: 'customrecord_btm_tt_tp_sx',
                filters:
                    [
                        ['custrecord_btm_tt_m_dh_yc_sx', 'anyof', requireOrder],
                        'AND',
                        ['custrecord_btm_tt_mh_tp', 'anyof', assembly],
                        'AND',
                        ['custrecord_btm_tt_mnsx', 'anyof', location],
                        'AND',
                        ['custrecord_btm_tt_wo_tam', 'noneof', '@NONE@'],
                        'AND',
                        ['custrecord_btm_tt_assembly_build_tam', 'noneof', '@NONE@']
                    ],
                columns:
                    [
                        search.createColumn({ name: 'custrecord_btm_tt_sl_thanh_pham', label: 'Số lượng thành phẩm' }),
                        search.createColumn({ name: 'custrecord_btm_tt_wo_tam', label: 'Work Order' })
                    ]
            });
            tp_sxSearchObj.run().each(function(result) {
                sum += +result.getValue('custrecord_btm_tt_sl_thanh_pham');
                woList.push(result.getValue('custrecord_btm_tt_wo_tam'));
                return true;
            });
            return { sum, woList };
        };
        const getDinhDa = (requireOrder, assemblyItem, location) => {
            let data = {};
            let dinh_daSearchObj = search.create({
                type: 'customrecord_btm_tt_don_yc_dinh_da',
                filters:
                    [
                        ['custrecord_btm_tt_don_yc_dinh_da_parent', 'anyof', requireOrder],
                        'AND',
                        ['custrecord_btm_tt_don_yc_dinh_da_ma_h_tp', 'anyof', assemblyItem],
                        'AND',
                        ['custrecord_btm_tt_don_yc_dinh_da_nm_sx', 'anyof', location]
                    ],
                columns:
                    [
                        search.createColumn({
                            name: 'scriptid',
                            sort: search.Sort.ASC,
                            label: 'Script ID'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_m_kh_d',
                            label: 'Mã hàng khách đặt'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_ma_h_tp',
                            label: 'Mã hàng thành phẩm'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_t_sm_dh',
                            label: 'Tổng số mét theo đơn hàng'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_dinh_da_phg_thc', label: 'Phương thức' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_l_h_kd',
                            label: 'Loại hình kinh doanh'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_loai_dv',
                            label: 'Loại dịch vụ'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_dinh_da_size', label: 'Size' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_dinh_da_dng_sai', label: 'Dung sai' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_sl_dat',
                            label: 'Số lượng khách đặt'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_dinh_da_dv_tinh', label: 'Đơn vị tính' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_dinh_da_sl_max', label: 'Số lượng MAX' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_dinh_da_sl_min', label: 'Số lượng MIN' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_depart',
                            label: 'Bộ phận sản xuất'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_dinh_da_nm_sx',
                            label: 'Nhà máy sản xuất'
                        })
                    ]
            });
            dinh_daSearchObj.run().each(function(result) {
                data.soLuongYeuCau = result.getValue('custrecord_btm_tt_don_yc_dinh_da_sl_dat');
                return true;
            });
            return data;
        };
        const getMay = (requireOrder, assemblyItem, location) => {
            let data = {};
            let yc_maySearchObj = search.create({
                type: 'customrecord_btm_tt_don_yc_may',
                filters:
                    [
                        ['custrecord_btm_tt_don_yc_may_parent', 'anyof', requireOrder],
                        'AND',
                        ['custrecord_btm_tt_don_yc_may_ma_hang_tp', 'anyof', assemblyItem],
                        'AND',
                        ['custrecord_btm_tt_don_yc_may_mnsx', 'anyof', location]
                    ],
                columns:
                    [
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_may_ma_kh_dat',
                            label: 'Mã hàng khách đặt'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_may_ma_hang_tp',
                            label: 'Mã hàng thành phẩm'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_may_tong_sm_dh',
                            label: 'Tổng số mét theo đơn hàng'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_phuong_thuc', label: 'Phương thức' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_may_loai_hnh_kd',
                            label: 'Loại hình kinh doanh'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_loai_dv', label: 'Loại dịch vụ' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_size', label: 'Size' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_dung_sai', label: 'Dung sai' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_may_sl_kh_dat',
                            label: 'Số lượng khách đặt'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_dv_tinh', label: 'Đơn vị tính' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_sl_max', label: 'Số lượng MAX' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_sl_min', label: 'Số lượng MIN' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_bpsx', label: 'Bộ phận sản xuất' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_may_mnsx', label: 'Nhà máy sản xuất' })
                    ]
            });
            yc_maySearchObj.run().each(function(result) {
                // .run().each has a limit of 4,000 results
                data.soLuongYeuCau = result.getValue('custrecord_btm_tt_don_yc_may_sl_kh_dat');
                return true;
            });
            return data;
        };
        const getIn = (requireOrder, assemblyItem, location) => {
            let data = {};
            let yc_inSearchObj = search.create({
                type: 'customrecord_btm_tt_don_yc_in',
                filters:
                    [
                        ['custrecord_btm_tt_don_yc_in_parent', 'anyof', requireOrder],
                        'AND',
                        ['custrecord_btm_tt_don_yc_in_ma_mat_hang', 'anyof', assemblyItem],
                        'AND',
                        ['custrecord_btm_tt_don_yc_in_mnsx', 'anyof', location]
                    ],
                columns:
                    [
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_loai_hang', label: 'Loại Hàng' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_ma_mat_hang', label: 'Mã mặt hàng' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_in_ms_quy_trinh',
                            label: 'Mã số quy trình'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_ma_mau', label: 'Mã màu' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_in_mm_kinh_doan',
                            label: 'Mã màu kinh doanh'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_in_mk_gia_thanh',
                            label: 'Mã khóa giá thành'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_in_mnxn_khoa',
                            label: 'Mã nhân viên xác nhận khóa '
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_sl_yc', label: 'Số Lượng Yêu Cầu' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_in_sl_da_xem_x',
                            label: 'Số Lượng Đã Xem Xét'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_date_start', label: 'Ngày Bắt Đầu' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_date_end', label: 'Ngày Kết Thúc' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_ma_cn_in', label: 'Mã Công Nghệ In' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_chuyen_kho', label: 'Chuyển kho' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_bpsx', label: 'Bộ phận sản xuất' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_in_mnsx', label: 'Nhà máy sản xuất' })
                    ]
            });
            yc_inSearchObj.run().each(function(result) {
                // .run().each has a limit of 4,000 results
                data.soLuongYeuCau = result.getValue('custrecord_btm_tt_don_yc_in_sl_da_xem_x');
                return true;
            });
            return data;
        };
        const getVaiThanhPham = (requireOrder, assemblyItem, location) => {
            let data = {};
            let yc_vtpSearchObj = search.create({
                type: 'customrecord_btm_tt_don_yc_vtp',
                filters:
                    [
                        ['custrecord_btm_tt_don_yc_vtp_parent', 'anyof', requireOrder],
                        'AND',
                        ['custrecord_btm_tt_don_yc_vtp_mhtp', 'anyof', assemblyItem],
                        'AND',
                        ['custrecord_btm_tt_don_yc_vtp_mnsx', 'anyof', location]
                    ],
                columns:
                    [
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_loai_hang', label: 'Loại Hàng' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_vtp_ma_kh_dat',
                            label: 'Mã hàng khách đặt'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_mhtp', label: 'Mã hàng thành phẩm' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_vtp_ms_quy_trin',
                            label: 'Mã số quy trình'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_ma_mau', label: 'Mã màu' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_mm_kd', label: 'Mã màu kinh doanh' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_vtp_mk_gia_than',
                            label: 'Mã khóa giá thành'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_vtp_mnv_xn_khoa',
                            label: 'Mã nhân viên xác nhận khóa '
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_sl_yc', label: 'Số Lượng Yêu Cầu' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_vtp_sl_da_xem_x',
                            label: 'Số Lượng Đã Xem Xét'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_loai_dh', label: 'Loại Đơn Hàng' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_don_yc_vtp_mcn_nhuom',
                            label: 'Mã Công Nghệ Nhuộm'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_chuyen_kho', label: 'Chuyển kho' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_bpsx', label: 'Bộ phận sản xuất' }),
                        search.createColumn({ name: 'custrecord_btm_tt_don_yc_vtp_mnsx', label: 'Nhà máy sản xuất' })
                    ]
            });
            yc_vtpSearchObj.run().each(function(result) {
                data.soLuongYeuCau = result.getValue('custrecord_btm_tt_don_yc_vtp_sl_da_xem_x');
                return true;
            });
            return data;
        };
        const getYCMoc = (requireOrder, assemblyItem, location) => {
            let data = {};
            let mSearchObj = search.create({
                type: 'customrecord_btm_tt_don_yc_m',
                filters:
                    [
                        ['custrecord_btm_tt_don_yc_m_parent.internalid', 'anyof', requireOrder],
                        'AND',
                        ['custrecord_btm_tt_dyc_m_hang_tp', 'anyof', assemblyItem],
                        'AND',
                        ['custrecord_btm_tt_dyc_m_mnsx', 'anyof', location]
                    ],
                columns:
                    [
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_ma_dinh_muc', label: 'Mã định mức' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_loai_yc', label: 'Loại Yêu Cầu' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_kho_may_det', label: 'Khổ Máy Dệt' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_hang_tp', label: 'Mã hàng thành phẩm' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_m_ma_hang_kd',
                            label: 'Mã Hàng Kinh Doanh'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_loai_bong', label: 'Loại Bông' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_sl_yc', label: 'Số Lượng Yêu Cầu' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_sl_dieu_bo', label: 'Số Lượng Điều Độ' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_mn', label: 'Mn' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_do_co_nhuom', label: 'Độ Co Nhuộm' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_loai_may_det', label: 'Loại máy dệt' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_m_ma_nv_cap_nhat',
                            label: 'Mã nhân viên cập nhật'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_ngay_cap_nhat', label: 'Ngày Cập Nhật' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_nhom_dong_goi', label: 'Nhóm đóng gói' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_qcdh', label: 'Qui cach dong hang' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_loai_bao_bi', label: 'Loại bao bì' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_nhom_sp', label: 'Nhóm sản phẩm' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_loai_vai', label: 'Loại vải' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_trang_phuc', label: 'Trang phục' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_bpsx', label: 'Bộ phận sản xuất' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_m_mnsx', label: 'Nhà máy sản xuất' })
                    ]
            });
            mSearchObj.run().each(function(result) {
                data.soLuongYeuCau = result.getValue('custrecord_btm_tt_dyc_m_sl_dieu_bo');
                // data.dinhMuc = result.getValue('custrecord_btm_tt_dyc_m_ma_dinh_muc');
                // data.loaiYeuCau = result.getValue('custrecord_btm_tt_dyc_m_loai_yc');
                // data.khoMayDet = result.getValue('custrecord_btm_tt_dyc_m_kho_may_det');
                // data.maHangThanhPham = result.getValue('custrecord_btm_tt_dyc_m_hang_tp');
                // data.maHangKinhDoanh = result.getValue('custrecord_btm_tt_dyc_m_ma_hang_kd');
                // data.loaiBong = result.getValue('custrecord_btm_tt_dyc_m_loai_bong');
                // data.soLuongDieuBo = result.getValue('custrecord_btm_tt_dyc_m_sl_dieu_bo');
                // data.mn = result.getValue('custrecord_btm_tt_dyc_m_mn');
                // data.doCoNhuom = result.getValue('custrecord_btm_tt_dyc_m_do_co_nhuom');
                // data.loaiMayDet = result.getValue('custrecord_btm_tt_dyc_m_loai_may_det');
                // data.maNhanVien = result.getValue('custrecord_btm_tt_dyc_m_ma_nv_cap_nhat');
                // data.ngayCapNhat = result.getValue('custrecord_btm_tt_dyc_m_ngay_cap_nhat');
                // data.nhomDongGoi = result.getValue('custrecord_btm_tt_dyc_m_nhom_dong_goi');
                // data.quiCachDongHang = result.getValue('custrecord_btm_tt_dyc_m_qcdh');
                // data.loaiBaoBi = result.getValue('custrecord_btm_tt_dyc_m_loai_bao_bi');
                // data.nhomSanPham = result.getValue('custrecord_btm_tt_dyc_m_nhom_sp');
                // data.loaiVai = result.getValue('custrecord_btm_tt_dyc_m_loai_vai');
                // data.trangPhuc = result.getValue('custrecord_btm_tt_dyc_m_trang_phuc');
                // data.bpsx = result.getValue('custrecord_btm_tt_dyc_m_bpsx');
                // data.mnsx = result.getValue('custrecord_btm_tt_dyc_m_mnsx');
                return true;
            });
            return data;
        };

        // lấy thông tin  Đơn hàng GC Sợi - TODO - Comment cho đúng format của function
        const getGiaCongSoi = (requireOrder, assemblyItem, location) => {
            let data = {};
            let customrecord_btm_tt_dyc_gcsSearchObj = search.create({
                type: 'customrecord_btm_tt_dyc_gcs',
                filters:
                    [
                        ['custrecord_btm_tt_dyc_gcs_parent', 'anyof', requireOrder],
                        'AND',
                        ['custrecord_btm_tt_dyc_gcs_mhtp', 'anyof', assemblyItem],
                         'AND',
                        ['custrecordcustrecord_btm_tt_don_yc_gcsoi', 'anyof', location]

                    ],
                columns:
                    [
                        search.createColumn({ name: 'scriptid', label: 'Script ID' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_gcs_loai_dh', label: 'Loại đơn hàng' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_gcs_mhtp', label: 'Mã hàng thành phẩm' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_gcs_sl_yc', label: 'Số lượng yêu cầu' }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_gcs_dinh_muc', label: 'Mã khóa định mức' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_gcs_ma_nhan_vien',
                            label: 'Mã nhân viên cập nhật'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_gcs_ngay_cap_nhat',
                            label: 'Ngày cập nhật'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_gcs_m_nv_ycsx',
                            label: 'Mã nhân viên yêu cầu sản xuất'
                        }),
                        search.createColumn({ name: 'custrecord_btm_tt_dyc_gcs_ngay_xem_xet', label: 'Ngày xem xét' }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_gcs_hieu_luc_xem_x',
                            label: 'Hiệu lực xem xét'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_gcs_mnvdv_thuc_hie',
                            label: 'Mã nhân viên đơn vị thực hiện xác nhận'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_gcs_ngay_xac_nhan',
                            label: 'Ngày xác nhận'
                        }),
                        search.createColumn({
                            name: 'custrecord_btm_tt_dyc_gcs_hieu_luc_xn',
                            label: 'Hiệu lực xác nhận'
                        }),
                             search.createColumn({
                                 name: 'custrecordcustrecord_btm_tt_don_yc_gcsoi',
                                 label: 'Nhà máy sản xuất'
                             })
                    ]
            });
            customrecord_btm_tt_dyc_gcsSearchObj.run().each(function(result) {
                data.soLuongYeuCau = result.getValue({
                    name: 'custrecord_btm_tt_dyc_gcs_sl_yc',
                    label: 'Số lượng yêu cầu'
                });
                return true;
            });
            return data;
        };

        /**
         * Lấy Item Receipt, Transfer Order có mã đơn hàng yêu cầu sản xuất
         * @param orderRequired
         * @return {*[]}
         */
        const getWareHouse = (orderRequired) => {
            let data = [];
            let transactionSearchObj = search.create({
                type: 'transaction',
                filters:
                    [
                        ['custbody_btm_tt_ma_don_yeu_cau', 'anyof', orderRequired],
                        'AND',
                        ['mainline', 'is', 'T'],
                        'AND',
                        ['type', 'anyof', 'TrnfrOrd', 'ItemRcpt']
                    ],
                columns:
                    [
                        search.createColumn({ name: 'tranid', label: 'Document Number' })
                    ]
            });
            transactionSearchObj.run().each(function(result) {
                data.push(result.id);
                return true;
            });
            return data;
        };

        const getWO = (requireOrder, assemblyItem, location) => {
            let data = {};
            let sxSearchObj = search.create({
                type: 'workorder',
                filters:
                    [
                        ['type', 'anyof', 'WorkOrd'],
                        'AND',
                        ['custbody_btm_tt_ma_don_yeu_cau', 'anyof', requireOrder],
                        'AND',
                        ['mainline', 'is', 'T'],
                        'AND',
                        ['item', 'anyof', assemblyItem],
                        'AND',
                        ['location', 'anyof', location],
                        'AND',
                        ['memorized', 'is', 'F']
                    ],
                columns:
                    [
                        search.createColumn({ name: 'tranid', label: 'Document Number' }),
                        search.createColumn({ name: 'location', label: 'Location' }),
                        search.createColumn({ name: 'department', label: 'Department' })
                    ]
            });
            sxSearchObj.run().each(function(result) {
                // if(!data[result.id]) data[result.id] = {};
                // data[result.id].id = result.id;
                // data[result.id].location = result.getValue('location');
                // data[result.id].department = result.getValue('department');
                data.id = result.id;
                return true;
            });
            log.debug('WO', data);
            return data;
        };

        /**
         * Kiểm tra mặt hàng bán thành phẩm - BTP
         * @param name
         * @return {null|number} Internal ID của BTP
         */
        const checkChildItem = (name) => {
            let data = null;
            let assemblyitemSearchObj = search.create({
                type: 'assemblyitem',
                filters:
                    [
                        ['type', 'anyof', 'Assembly'],
                        'AND',
                        ['name', 'is', name]
                    ],
                columns:
                    [
                        search.createColumn({
                            name: 'itemid',
                            sort: search.Sort.ASC,
                            label: 'Name'
                        })
                    ]
            });
            assemblyitemSearchObj.run().each(function(result) {
                data = result.id;
                return true;
            });
            log.debug('BTP', data);
            return data;
        };

        const checkChildBOM = (name) => {
            let data;
            let searchObj = search.create({
                type: search.Type.BOM,
                filters:
                    [
                        ['name', 'is', name]
                    ],
                columns:
                    [
                        search.createColumn({
                            name: 'internalid',
                            sort: search.Sort.ASC,
                            label: 'ID'
                        })
                    ]
            });
            searchObj.run().each(function(result) {
                data = result.getValue({
                    name: 'internalid',
                    sort: search.Sort.ASC,
                    label: 'ID'
                });
                return true;
            });
            return data;
        };

        const checkChildBOMRev = (name) => {
            let data;
            let searchObj = search.create({
                type: search.Type.BOM_REVISION,
                filters:
                    [
                        ['name', 'is', name]
                    ],
                columns:
                    [
                        search.createColumn({
                            name: 'internalid',
                            sort: search.Sort.ASC,
                            label: 'ID'
                        })
                    ]
            });
            searchObj.run().each(function(result) {
                data = result.getValue({
                    name: 'internalid',
                    sort: search.Sort.ASC,
                    label: 'ID'
                });
                return true;
            });
            return data;
        };

        const getBOMRev = (name) => {
            let data = {};
            let searchObj = search.create({
                type: search.Type.BOM_REVISION,
                filters:
                    [
                        ['name', 'is', name]
                    ],
                columns:
                    [
                        search.createColumn({
                            name: 'internalid',
                            sort: search.Sort.ASC,
                            label: 'ID'
                        }),
                        search.createColumn({
                            name: 'effectivestartdate',
                            label: 'EFFECTIVE START DATE'
                        }),
                        search.createColumn({
                            name: 'effectiveenddate',
                            label: 'EFFECTIVE END DATE'
                        })
                    ]
            });
            searchObj.run().each(function(result) {
                data.startDate = result.getValue('effectivestartdate');
                data.endDate = result.getValue('effectiveenddate');
                return true;
            });
            return data;
        };


        const findChildWO = (id) => {
            let data;
            let workorderSearchObj = search.create({
                type: 'workorder',
                filters:
                    [
                        ['type', 'anyof', 'WorkOrd'],
                        'AND',
                        ['mainline', 'is', 'T'],
                        'AND',
                        ['createdfrom', 'anyof', id]
                    ],
                columns:
                    [
                        search.createColumn({ name: 'tranid', label: 'Document Number' })
                    ]
            });
            workorderSearchObj.run().each(function(result) {
                data = result.id;
                return true;
            });
            return data;
        };

        const findAssemblyCost = (id) => {
            let data = [], totalRemaining = 0;
            let searchObj = search.create({
                type: 'assemblybuild',
                filters:
                    [
                        ['type', 'anyof', 'Build'],
                        'AND',
                        ['createdfrom', 'anyof', id],
                        'AND',
                        ['mainline', 'is', 'T'],
                        'AND',
                        ['custbody_btm_mc_used_by_mc', 'is', 'T'],
                        'AND',
                        ['custbody_btm_mc_ass_rem_qty', 'greaterthan', '0']
                    ],
                columns:
                    [
                        search.createColumn({ name: 'trandate', sort: search.Sort.ASC }),
                        search.createColumn({ name: 'custbody_btm_mc_ass_rem_qty', label: 'Remaining Qty' })
                    ]
            });
            searchObj.run().each(function(result) {

                let remainingQty = +result.getValue('custbody_btm_mc_ass_rem_qty');
                let obj = {};
                obj.id = result.id;
                obj.remainingQty = remainingQty;
                data.push(obj);

                totalRemaining += remainingQty;
                return true;
            });
            return { data, totalRemaining };
        };

        const checkNegativeInventory = (items, location) => {
            let data = [];
            let itemSearchObj = search.create({
                type: 'item',
                filters:
                    [
                        ['internalid', 'anyof', ...items],
                        'AND',
                        ['inventorylocation', 'anyof', location]
                    ],
                columns:
                    [
                        search.createColumn({ name: 'itemid', label: 'Name' }),
                        search.createColumn({ name: 'locationquantityonhand', label: 'Location On Hand' }),
                        search.createColumn({ name: 'inventorylocation', label: 'Inventory Location' })
                    ]
            });
            itemSearchObj.run().each(function(result) {
                let qty = +result.getValue('locationquantityonhand') || 0;
                let item = result.getValue('itemid');
                if (qty <= 0) {
                    data.push(item);
                }
                return true;
            });
            return data;
        };

        const findBOMDefault = (assembly) => {
            let data;
            let bomSearchObj = search.create({
                type: 'bom',
                filters:
                    [
                        ['assemblyitem.assembly', 'anyof', assembly],
                        'AND',
                        ['assemblyitem.default', 'is', 'T']
                    ],
                columns:
                    [
                        search.createColumn({ name: 'name', label: 'Name' }),
                        search.createColumn({ name: 'revisionname', label: 'Revision : Name' }),
                        search.createColumn({
                            name: 'default',
                            join: 'assemblyItem',
                            label: 'Default'
                        })
                    ]
            });
            bomSearchObj.run().each(function(result) {
                data = result.id;
                // .run().each has a limit of 4,000 results
                return true;
            });
            return data;
        };
        const handleOldBOM = (assemblyItem) => {
            //Tìm BOM cũ đã check default để bỏ check
            const oldBOM = findBOMDefault(assemblyItem);

            if (oldBOM) {
                let oldBOMRcrd = record.load({
                    type: record.Type.BOM,
                    id: oldBOM,
                    isDynamic: true
                });
                let itemLine = oldBOMRcrd.findSublistLineWithValue({
                    sublistId: 'assembly',
                    fieldId: 'assembly',
                    value: assemblyItem
                });

                if (itemLine > -1) {
                    oldBOMRcrd.removeLine({
                        sublistId: 'assembly',
                        line: itemLine,
                        ignoreRecalc: true
                    });
                    oldBOMRcrd.selectNewLine('assembly');
                    oldBOMRcrd.setCurrentSublistValue({
                        sublistId: 'assembly',
                        fieldId: 'assembly',
                        value: assemblyItem
                    });
                    oldBOMRcrd.setCurrentSublistValue({
                        sublistId: 'assembly',
                        fieldId: 'masterdefault',
                        value: false
                    });
                    oldBOMRcrd.commitLine('assembly');
                    oldBOMRcrd.save();
                }
            }
        };
        return { getInputData, map, reduce, summarize };

    });
