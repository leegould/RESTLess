# -*- coding: utf-8 -*-

# Form implementation generated from reading ui file 'restless.ui'
#
# Created: Thu Dec  4 20:39:15 2014
#      by: PyQt5 UI code generator 5.3.2
#
# WARNING! All changes made in this file will be lost!

from PyQt5 import QtCore, QtGui, QtWidgets

class Ui_MainWindowRESTLess(object):
    def setupUi(self, MainWindowRESTLess):
        MainWindowRESTLess.setObjectName("MainWindowRESTLess")
        MainWindowRESTLess.resize(800, 600)
        self.centralwidget = QtWidgets.QWidget(MainWindowRESTLess)
        self.centralwidget.setObjectName("centralwidget")
        self.pushButtonSend = QtWidgets.QPushButton(self.centralwidget)
        self.pushButtonSend.setGeometry(QtCore.QRect(700, 40, 93, 28))
        self.pushButtonSend.setObjectName("pushButtonSend")
        self.lineEditUrl = QtWidgets.QLineEdit(self.centralwidget)
        self.lineEditUrl.setGeometry(QtCore.QRect(220, 10, 571, 22))
        self.lineEditUrl.setObjectName("lineEditUrl")
        self.labelUrl = QtWidgets.QLabel(self.centralwidget)
        self.labelUrl.setGeometry(QtCore.QRect(140, 10, 53, 16))
        self.labelUrl.setObjectName("labelUrl")
        self.textEditResults = QtWidgets.QTextEdit(self.centralwidget)
        self.textEditResults.setGeometry(QtCore.QRect(220, 80, 571, 301))
        self.textEditResults.setObjectName("textEditResults")
        MainWindowRESTLess.setCentralWidget(self.centralwidget)
        self.menubar = QtWidgets.QMenuBar(MainWindowRESTLess)
        self.menubar.setGeometry(QtCore.QRect(0, 0, 800, 26))
        self.menubar.setObjectName("menubar")
        MainWindowRESTLess.setMenuBar(self.menubar)
        self.statusbar = QtWidgets.QStatusBar(MainWindowRESTLess)
        self.statusbar.setObjectName("statusbar")
        MainWindowRESTLess.setStatusBar(self.statusbar)

        self.retranslateUi(MainWindowRESTLess)
        QtCore.QMetaObject.connectSlotsByName(MainWindowRESTLess)

    def retranslateUi(self, MainWindowRESTLess):
        _translate = QtCore.QCoreApplication.translate
        MainWindowRESTLess.setWindowTitle(_translate("MainWindowRESTLess", "RESTLess"))
        self.pushButtonSend.setText(_translate("MainWindowRESTLess", "Send"))
        self.labelUrl.setText(_translate("MainWindowRESTLess", "URL"))

